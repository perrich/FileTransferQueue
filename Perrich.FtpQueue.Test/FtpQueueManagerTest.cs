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

            CheckFile(SrcFile1);
            CheckFile(StreamFile1);

            queue = new FtpQueue(QueueName);
            queue.Enqueue(new FtpItem { DestPath = DestFile1, SrcPath = SrcFile1 });
            queue.Enqueue(new FtpItem { DestPath = "./1.txt", Identifier = "1" });
            queue.Enqueue(new FtpItem { DestPath = "./2.txt", Identifier = "2" });

            A.CallTo(() => queueRepository.Load(QueueName)).Returns(queue);

            fakeStream = new FileStream(StreamFile1, FileMode.Open, FileAccess.Read);

            A.CallTo(() => system.GetStream(A<string>.Ignored)).Returns(fakeStream);

            manager = new FtpQueueManager(QueueName, queueRepository, system, provider);
        }

        private static void CheckFile(string filename)
        {
            if (File.Exists(filename))
                return;

            using (File.CreateText(filename))
            {
            }
        }

        [Test]
        public void ShouldInitManagerCallQueueLoading()
        {
            manager.Init();
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ShouldInitAndApplyProcessesQueueAfterLoadingIt()
        {
            manager.InitAndApply();
            A.CallTo(() => queueRepository.Load(QueueName)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(3));
            A.CallTo(() => provider.Send(A<Stream>.Ignored, DestFile1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile3)).MustHaveHappened(Repeated.Exactly.Once);
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
        public void ShouldApplyAndSaveProcessesQueueBeforeSavingIt()
        {
            manager.Init();
            manager.ApplyAndSave();
            A.CallTo(() => queueRepository.Save(queue)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(A<Stream>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(3));
            A.CallTo(() => provider.Send(A<Stream>.Ignored, DestFile1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile2)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => provider.Send(fakeStream, DestFile3)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}