using System;
using NUnit.Framework;

namespace Perrich.FtpQueue.Test
{
    public class FtpQueueTest
    {
        private const string QueueName = "MySampleQueue";

        [Test]
        public void ShouldFtpQueueHasAName()
        {
            var queue = new FtpQueue(QueueName);
            Assert.AreEqual(QueueName, queue.Name);
        }

        [Test]
        public void ShouldEmptyQueueReturnNullWhenDequeue()
        {
            var queue = new FtpQueue(QueueName);
            Assert.Null(queue.Dequeue());
        }

        [Test]
        public void ShouldEmptyQueueReturnEmptyListWhenDequeue()
        {
            var queue = new FtpQueue(QueueName);
            var list = queue.FlushItems();
            Assert.NotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void ShouldRetrieveEnqueuedFtpItem()
        {
            var queue = new FtpQueue(QueueName);
            var item = new FtpItem {DestPath = "./1.txt", Identifier = "1"};
            queue.Enqueue(item);
            Assert.AreSame(item, queue.Dequeue());
            Assert.Null(queue.Dequeue(), "Only one value was in the queue");
        }

        [Test]
        public void ShouldRetrieveEnqueuedFtpItemsInTheSameOrder()
        {
            var queue = new FtpQueue(QueueName);
            var item = new FtpItem { DestPath = "./1.txt", Identifier = "1" };
            var item2 = new FtpItem { DestPath = "./2.txt", Identifier = "2" };
            queue.Enqueue(item);
            queue.Enqueue(item2);
            Assert.AreSame(item, queue.Dequeue());
            Assert.AreSame(item2, queue.Dequeue());
            Assert.Null(queue.Dequeue(), "Only two values were in the queue");
        }

        [Test]
        public void ShouldListAllEnqueuedFtpItemsInTheSameOrder()
        {
            var queue = new FtpQueue(QueueName);
            var item = new FtpItem { DestPath = "./1.txt", Identifier = "1" };
            var item2 = new FtpItem { DestPath = "./2.txt", Identifier = "2" };
            var item3 = new FtpItem { DestPath = "./3.txt", Identifier = "3" };
            queue.Enqueue(item);
            queue.Enqueue(item2);
            queue.Enqueue(item3);

            var list = queue.FlushItems();
            Assert.Null(queue.Dequeue(), "Queue should be empty after a flush");
            Assert.AreEqual(3, list.Count);
            Assert.AreSame(item, list[0]);
            Assert.AreSame(item2, list[1]);
            Assert.AreSame(item3, list[2]);
        }

        [Test]
        public void ShouldEnqueueThrowExceptionIfDestinationFilenameNotDefined()
        {
            var queue = new FtpQueue(QueueName);
            var item = new FtpItem { DestPath = "./", Identifier = "1" };
            Assert.Throws<ArgumentException>(() => queue.Enqueue(item));
        }

        [Test]
        public void ShouldCheckIfQueueContainsADestinationFilename()
        {
            const string filename = "mysamplefilename.zip";
            var queue = new FtpQueue(QueueName);
            var item = new FtpItem { DestPath = "./" + filename, Identifier = "1" };
            queue.Enqueue(item);
            Assert.True(queue.Contains(filename));
        }
    }
}