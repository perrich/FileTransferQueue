using NUnit.Framework;

namespace Perrich.FileTransferQueue.Test
{
    public class FileItemTest
    {
        private const string Identifier = "MyID";
        private const string DestPath = "d:\\foo\\sample.txt";
        private const string SrcPath = "c:\\foo\\sample.txt";

        [Test]
        public void ShouldFileItemCanContainAnIdentifier()
        {
            var item = new FileItem { DestPath = DestPath, Identifier = Identifier };
            Assert.AreEqual(item.DestPath, DestPath);
            Assert.AreEqual(item.Identifier, Identifier);
            Assert.Null(item.SrcPath);
            Assert.AreEqual(string.Format("Identifier: {0}, DestPath: {1}", item.Identifier, item.DestPath), item.ToString());
        }

        [Test]
        public void ShouldFileItemCanContainASourcePath()
        {
            var item = new FileItem { DestPath = DestPath, SrcPath = SrcPath };
            Assert.AreEqual(item.DestPath, DestPath);
            Assert.AreEqual(item.SrcPath, SrcPath);
            Assert.Null(item.Identifier);
            Assert.AreEqual(string.Format("SrcPath: {0}, DestPath: {1}", item.SrcPath, item.DestPath), item.ToString());
        }

        [Test]
        public void ShouldFileItemCannotContainSourcePathAndIdentifierInTheSameTime()
        {
            var item = new FileItem { DestPath = DestPath, SrcPath = SrcPath };
            //set identifier and check
            item.Identifier = Identifier;
            Assert.AreEqual(item.Identifier, Identifier);
            Assert.Null(item.SrcPath);
            //set src path and check
            item.SrcPath = SrcPath;
            Assert.AreEqual(item.SrcPath, SrcPath);
            Assert.Null(item.Identifier);
        }
    }
}