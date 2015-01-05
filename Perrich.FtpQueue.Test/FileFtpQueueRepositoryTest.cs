using System.IO;
using NUnit.Framework;

namespace Perrich.FtpQueue.Test
{
    public class FileFtpQueueRepositoryTest
    {
        [Test]
        public void ShouldInitializeAnEmptyQueueForNewerQueue()
        {
            const string queueName = "MySampleQueue";
            var repo = new FileFtpQueueRepository(".");
            File.Delete(GetFileName(queueName));
            var queue = repo.Load(queueName);

            Assert.AreEqual(0, queue.FlushItems().Count);
        }

        [Test]
        public void ShouldLoadAQueue()
        {
            const string queueName = "MySampleLoadQueue";
            var filename = GetFileName(queueName);

            var repo = new FileFtpQueueRepository(".");
            File.Delete(filename);
            using (var w = File.CreateText(filename))
            {
                w.WriteLine("[ { 'DestPath': './mySample.txt', 'SrcPath': 'mySample.txt' }, { 'DestPath': 'a.txt', 'Identifier': 'a' }, { 'DestPath': 'b.txt', 'Identifier': 'b' }]");
                w.Close();
            }
            var queue = repo.Load(queueName);

            var item = queue.Dequeue();
            Assert.NotNull(item);
            Assert.AreEqual("./mySample.txt", item.DestPath);
            Assert.AreEqual("mySample.txt", item.SrcPath);
            Assert.Null(item.Identifier);

            item = queue.Dequeue();
            Assert.NotNull(item);
            Assert.AreEqual("a.txt", item.DestPath);
            Assert.AreEqual("a", item.Identifier);
            Assert.Null(item.SrcPath);

            item = queue.Dequeue();
            Assert.NotNull(item);
            Assert.AreEqual("b.txt", item.DestPath);
            Assert.AreEqual("b", item.Identifier);
            Assert.Null(item.SrcPath);

            item = queue.Dequeue();
            Assert.Null(item);
        }

        [Test]
        public void ShouldSaveAQueue()
        {
            const string queueName = "MySampleSaveQueue";
            var filename = GetFileName(queueName);

            var queue = new FtpQueue(queueName);
            queue.Enqueue(new FtpItem { DestPath = "./destfile.txt", SrcPath = "srcfile.txt" });
            queue.Enqueue(new FtpItem { DestPath = "./1.txt", Identifier = "1" });
            queue.Enqueue(new FtpItem { DestPath = "./1.txt", Identifier = "2" });

            var repo = new FileFtpQueueRepository(".");
            File.Delete(filename);
            repo.Save(queue);

            Assert.AreEqual(0, queue.FlushItems().Count, "After a save, the queue should be empty");
            Assert.True(File.Exists(filename));

            Assert.AreEqual("[{\"SrcPath\":\"srcfile.txt\",\"DestPath\":\"./destfile.txt\"},{\"Identifier\":\"1\",\"DestPath\":\"./1.txt\"},{\"Identifier\":\"2\",\"DestPath\":\"./1.txt\"}]", File.ReadAllText(filename));
        }

        private static string GetFileName(string queueName)
        {
            return queueName + ".queue";
        }
    }
}
