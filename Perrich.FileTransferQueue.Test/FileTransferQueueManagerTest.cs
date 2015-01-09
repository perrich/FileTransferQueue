using System;
using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using NUnit.Framework;

namespace Perrich.FileTransferQueue.Test
{
    public class FileTransferQueueManagerTest
    {
        private const string QueueName = "MySampleQueue";
        private const string DestFile1 = "./destfile.txt";
        private const string DestFile2 = "./1.txt";
        private const string DestFile3 = "./2.txt";
        private const string Identifier1 = "A1";
        private const string Identifier2 = "KEY_2";

        private const string SrcFile1 = "FileTransferQueueManagerTest1.txt";
        private const string StreamFile1 = "FileTransferQueueManagerTest2.txt";

        private IQueueRepository queueRepository;
        private IFileSystem system;
        private ISendingProvider provider;
        private FileTransferQueueManager manager;
        private FileTransferQueue queue;
        private Stream fakeStream;

        private Dictionary<FileTransferQueueManager.NotificationType, IList<FileItem>> receivedEvents = new Dictionary<FileTransferQueueManager.NotificationType, IList<FileItem>>();

        [SetUp]
        public void Init()
        {
            queueRepository = A.Fake<IQueueRepository>();
            system = A.Fake<IFileSystem>();
            provider = A.Fake<ISendingProvider>();

            CreateFile(SrcFile1);
            CreateFile(StreamFile1);

            fakeStream = new FileStream(StreamFile1, FileMode.Open, FileAccess.Read);

            queue = new FileTransferQueue(QueueName);
            queue.Enqueue(new FileItem { DestPath = DestFile1, SrcPath = SrcFile1 });
            queue.Enqueue(new FileItem { DestPath = "./1.txt", Identifier = Identifier1 });
            queue.Enqueue(new FileItem { DestPath = "./2.txt", Identifier = Identifier2 });

            A.CallTo(() => queueRepository.Load(QueueName)).Returns(queue);

            A.CallTo(() => system.GetStream(A<string>.Ignored)).Returns(fakeStream);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).Returns(true);

            manager = new FileTransferQueueManager(QueueName, queueRepository, system, provider);
            manager.NotificationRaised += manager_NotificationRaised;
        }

        [TearDown]
        public void Dispose()
        {
            manager.NotificationRaised -= manager_NotificationRaised;
        }

        [Test]
        public void ShouldInitManagerCallQueueLoading()
        {
            manager.Init();
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ShouldInitAndApplyProcessQueueAfterLoadingIt()
        {
            manager.InitAndApply();
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            AssertQueueIsFullyApplied();
        }

        [Test]
        public void ShouldApplyProcessQueue()
        {
            manager.Init();
            manager.ApplyQueue();
            AssertQueueIsFullyApplied();
        }

        [Test]
        public void ShouldSaveManagerCallQueueSaving()
        {
            manager.Init();
            manager.Save();
            A.CallTo(() => queueRepository.Save(queue)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ShouldApplyAndSaveProcessQueueBeforeSavingIt()
        {
            manager.Init();
            manager.ApplyAndSave();
            AssertQueueIsFullyApplied();
            A.CallTo(() => queueRepository.Save(queue)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldEnqueueFileIfSendIsRejected()
        {
            var q = new FileTransferQueue(QueueName);

            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).Returns(false);
            A.CallTo(() => queueRepository.Load(QueueName)).Returns(q);
            manager.Init();
            manager.TryToSend(SrcFile1, DestFile1);
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(q);
            var list = q.FlushItems();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(SrcFile1, list[0].SrcPath);
            Assert.AreEqual(DestFile1, list[0].DestPath);
        }

        [Test]
        public void ShouldEnqueueStreamAndNotifyIfSendIsRejected()
        {
            var q = new FileTransferQueue(QueueName);

            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).Returns(false);
            A.CallTo(() => queueRepository.Load(QueueName)).Returns(q);
            A.CallTo(() => system.SaveStream(fakeStream)).Returns(Identifier1);

            manager.Init();

            manager.TryToSend(fakeStream, DestFile1);
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.SaveStream(fakeStream)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(q);
            var list = q.FlushItems();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(Identifier1, list[0].Identifier);
            Assert.AreEqual(DestFile1, list[0].DestPath);

            AssertOnlyOneNotificationReceived(FileTransferQueueManager.NotificationType.Warn, Identifier1);
        }

        [Test]
        public void ShouldNotifyErrorAndProcessOthersWhenApplyProcessQueue()
        {
            A.CallTo(() => system.GetStream(Identifier1)).Throws(new FileSystemException(FileSystemException.ActionType.Delete, "something goes wrong"));
            manager.InitAndApply();

            //only the first is rejected
            A.CallTo(() => system.Delete(Identifier1)).MustNotHaveHappened();
            A.CallTo(() => system.Delete(Identifier2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.GetStream(Identifier2)).MustHaveHappened(Repeated.Exactly.Once);

            AssertOnlyOneNotificationReceived(FileTransferQueueManager.NotificationType.Error, Identifier1);
        }

        [Test]
        public void ShouldManagerThrowExceptionIfSourceStreamIsNull()
        {
            Assert.Throws<ArgumentException>(() => manager.TryToSend((Stream)null, DestFile1));
        }


        [Test]
        public void ShouldManagerThrowExceptionIfSourceFileIsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => manager.TryToSend((string)null, DestFile1));
            Assert.Throws<ArgumentException>(() => manager.TryToSend(string.Empty, DestFile1));
        }

        [Test]
        public void ShouldManagerThrowExceptionIfDestinationFileIsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => manager.TryToSend(SrcFile1, null));
            Assert.Throws<ArgumentException>(() => manager.TryToSend(SrcFile1, string.Empty));
            Assert.Throws<ArgumentException>(() => manager.TryToSend(fakeStream, null));
            Assert.Throws<ArgumentException>(() => manager.TryToSend(fakeStream, string.Empty));
        }

        void manager_NotificationRaised(FileTransferQueueManager.NotificationType type, FileItem item)
        {
            if (!receivedEvents.ContainsKey(type))
            {
                receivedEvents.Add(type, new List<FileItem>());
            }
            receivedEvents[type].Add(item);
        }

        private void AssertQueueIsFullyApplied()
        {
            A.CallTo(() => system.GetStream(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);
            A.CallTo(() => system.GetStream(Identifier1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.GetStream(Identifier2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(3));
            A.CallTo(() => provider.Send(A<Stream>.Ignored, DestFile1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile3)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.Delete(Identifier1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.Delete(Identifier2)).MustHaveHappened(Repeated.Exactly.Once);
        }

        private void AssertOnlyOneNotificationReceived(FileTransferQueueManager.NotificationType type, string identifier)
        {
            Assert.True(receivedEvents.ContainsKey(type));
            Assert.AreEqual(1, receivedEvents[type].Count);
            Assert.AreEqual(identifier, receivedEvents[type][0].Identifier);
        }

        private static void CreateFile(string filename)
        {
            if (File.Exists(filename))
                return;

            using (File.CreateText(filename))
            {
            }
        }
    }
}