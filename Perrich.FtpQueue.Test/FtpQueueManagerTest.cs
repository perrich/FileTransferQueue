using System.IO;
using FakeItEasy;
using NUnit.Framework;

namespace Perrich.FtpQueue.Test
{
    public class FtpQueueManagerTest
    {
        private const string QueueName = "MySampleQueue";
        private const string DestFile1 = "./destfile.txt";
        private const string DestFile2 = "./1.txt";
        private const string DestFile3 = "./2.txt";
        private const string Identifier1 = "A1";
        private const string Identifier2 = "KEY_2";

        private const string SrcFile1 = "FtpQueueManagerTest1.txt";
        private const string StreamFile1 = "FtpQueueManagerTest2.txt";

        private IFtpQueueRepository queueRepository;
        private IFileSystem system;
        private ISendingProvider provider;
        private FtpQueueManager manager;
        private FtpQueue queue;
        private Stream fakeStream;

        [SetUp]
        public void Init()
        {
            queueRepository = A.Fake<IFtpQueueRepository>();
            system = A.Fake<IFileSystem>();
            provider = A.Fake<ISendingProvider>();

            CreateFile(SrcFile1);
            CreateFile(StreamFile1);

            fakeStream = new FileStream(StreamFile1, FileMode.Open, FileAccess.Read);

            queue = new FtpQueue(QueueName);
            queue.Enqueue(new FtpItem { DestPath = DestFile1, SrcPath = SrcFile1 });
            queue.Enqueue(new FtpItem { DestPath = "./1.txt", Identifier = Identifier1 });
            queue.Enqueue(new FtpItem { DestPath = "./2.txt", Identifier = Identifier2 });

            A.CallTo(() => queueRepository.Load(QueueName)).Returns(queue);

            A.CallTo(() => system.GetStream(A<string>.Ignored)).Returns(fakeStream);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).Returns(true);

            manager = new FtpQueueManager(QueueName, queueRepository, system, provider);
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
            CheckQueueIsFullyApplied();
        }

        [Test]
        public void ShouldApplyProcessQueue()
        {
            manager.Init();
            manager.ApplyQueue();
            CheckQueueIsFullyApplied();
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
            CheckQueueIsFullyApplied();
            A.CallTo(() => queueRepository.Save(queue)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldEnqueueFileIfSendIsRejected()
        {
            var queue = new FtpQueue(QueueName);

            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).Returns(false);
            A.CallTo(() => queueRepository.Load(QueueName)).Returns(queue);
            manager.Init();
            manager.TryToSend(SrcFile1, DestFile1);
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(queue);
            var list = queue.FlushItems();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(SrcFile1, list[0].SrcPath);
            Assert.AreEqual(DestFile1, list[0].DestPath);
        }

        [Test]
        public void ShouldEnqueueStreamIfSendIsRejected()
        {
            var queue = new FtpQueue(QueueName);

            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).Returns(false);
            A.CallTo(() => queueRepository.Load(QueueName)).Returns(queue);
            A.CallTo(() => system.SaveStream(fakeStream)).Returns(Identifier1);

            manager.Init();

            manager.TryToSend(fakeStream, DestFile1);
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.SaveStream(fakeStream)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(queue);
            var list = queue.FlushItems();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(Identifier1, list[0].Identifier);
            Assert.AreEqual(DestFile1, list[0].DestPath);
        }

        private void CheckQueueIsFullyApplied()
        {
            A.CallTo(() => system.GetStream(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);
            A.CallTo(() => system.GetStream(Identifier1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => system.GetStream(Identifier2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(3));
            A.CallTo(() => provider.Send(A<Stream>.Ignored, DestFile1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile3)).MustHaveHappened(Repeated.Exactly.Once);
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