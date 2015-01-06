using System;
using System.IO;
using System.Text;
using log4net;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Implementation of a local file respository.
    /// All files will be saved with a unique identifier in a single directory
    /// </summary>
    public class LocalFileSystem : IFileSystem
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocalFileSystem).FullName);

        private readonly string dirPath;

        public LocalFileSystem(string dirPath)
        {
            this.dirPath = dirPath;
        }

        public string SaveFile(string fullPath)
        {
            try
            {
                var id = GetUniqueId(fullPath);
                var path = Path.Combine(dirPath, id);
                File.Copy(fullPath, path);

                return id;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Cannot save file named \"{0}\"", fullPath), ex);
                throw new FileSystemException(FileSystemException.ActionType.Write,
                    string.Format("Cannot write the file \"{0}\" in the file system.", fullPath));
            }
        }

        public string SaveStream(Stream stream)
        {
            try
            {
                var fullPath = Convert.ToString(Guid.NewGuid().ToByteArray()) + ".stream"; // simulate a fullpath
                var id = GetUniqueId(fullPath);
                var path = Path.Combine(dirPath, id);
                using (FileStream fileStream = File.Create(path))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

                return id;
            }
            catch (Exception ex)
            {
                Log.Error("Cannot save wanted stream!", ex);
                throw new FileSystemException(FileSystemException.ActionType.Write,
                    "Cannot write the stream in the file system.");
            }
        }

        public Stream GetStream(string identifier)
        {
            var path = Path.Combine(dirPath, identifier);

            if (!File.Exists(path))
                throw new FileSystemException(FileSystemException.ActionType.Read,
                    string.Format("The identifier \"{0}\" does not exists in the file system.", identifier));

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        private static string GetUniqueId(string fullPath)
        {
            var sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create(fullPath))
            {
                foreach (var t in md5.Hash)
                {
                    sb.Append(t.ToString("x2"));
                }
            }

            sb.Append("-");
            sb.Append(DateTime.Now.Ticks.ToString("x"));
            sb.Append(".");
            sb.Append(Path.GetExtension(fullPath));

            return sb.ToString();
        }
    }
}