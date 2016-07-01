using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Perrich.FileTransferQueue.Test
{
    public class LocalFileSystemTest
    {
        private const string Filename = "myfile.sample";
        private string identifier;
        private LocalFileSystem system;

        [SetUp]
        public void Init()
        {
            system = new LocalFileSystem(".");
            CreateFile(Filename);
        }

        [TearDown]
        public void Dispose()
        {
            if (File.Exists(Filename))
                File.Delete(Filename);

            if (identifier != null && File.Exists(identifier))
                File.Delete(identifier);

            foreach (var file in Directory.GetFiles(".", "*.stream"))
            {
                File.Delete(file);
            }
        }

        [Test]
        public void ShouldLocalFileSystemSaveAndRetrieveSavedFile()
        {
            identifier = system.SaveFile(Filename);
            using (var stream = system.GetStream(identifier))
            {
                Assert.NotNull(stream);
            }
            Assert.True(File.Exists(identifier));
        }

        [Test]
        public void ShouldLocalFileSystemDeleteSavedFile()
        {
            identifier = system.SaveFile(Filename);
            system.Delete(identifier);
            Assert.False(File.Exists(identifier));
        }

        [Test]
        public void ShouldLocalFileSystemDoNothingIfTryingToDeleteUnknownFile()
        {
            var id = "myfilewhichdoesnotexists.something";
            system.Delete(id);
            Assert.False(File.Exists(id));
        }

        [Test]
        public void ShouldLocalFileSystemSaveAndRetrieveSavedStream()
        {
            using (var srcStream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                identifier = system.SaveStream(srcStream);
                using (var stream = system.GetStream(identifier))
                {
                    Assert.NotNull(stream);
                }
                Assert.True(File.Exists(identifier));
            }
        }

        [Test]
        public void ShouldCreateUniqueStreamIdentifier()
        {
            using (var srcStream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                int number = 20;
                var list = new List<string>();
                foreach(int i in Enumerable.Range(1, number)){
                    list.Add(system.SaveStream(srcStream));
                };

                Assert.AreEqual(list.Distinct().Count(), number);
            }
        }

        [Test]
        public void ShouldLocalFileSystemThrowExceptionIfIdentifierDoesNotExists()
        {
            var exception = Assert.Catch<FileSystemException>(() => system.GetStream("notavalidfilename.zzz"));
            Assert.AreEqual(FileSystemException.ActionType.Read, exception.Type);
        }

        [Test]
        public void ShouldLocalFileSystemThrowExceptionIfFileDoesNotExists()
        {
            var exception = Assert.Catch<FileSystemException>(() => system.SaveFile("notavalidfilename.zzz"));
            Assert.AreEqual(FileSystemException.ActionType.Write, exception.Type);
        }

        [Test]
        public void ShouldLocalFileSystemThrowExceptionIfStreamIsNull()
        {
            var exception = Assert.Catch<FileSystemException>(() => system.SaveStream(null));
            Assert.AreEqual(FileSystemException.ActionType.Write, exception.Type);
        }

        [Test]
        public void ShouldLocalFileSystemThrowExceptionIfStreamCannotBeCreated()
        {
            using (new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var exception = Assert.Catch<FileSystemException>(() => system.GetStream(identifier));
                Assert.AreEqual(FileSystemException.ActionType.Read, exception.Type);
            }
        }

        [Test]
        public void ShouldLocalFileSystemThrowExceptionIfStreamCannotBeDeleted()
        {
            using (new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var exception = Assert.Catch<FileSystemException>(() => system.Delete(Filename));
                Assert.AreEqual(FileSystemException.ActionType.Delete, exception.Type);
            }
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
