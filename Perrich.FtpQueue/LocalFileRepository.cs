using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Implementation of a local file respository.
    /// All files will be saved with a unique identifier in a single directory
    /// </summary>
    public class LocalFileRepository : IFileRepository
    {
        private readonly string dirPath;

        public LocalFileRepository(string dirPath)
        {
            this.dirPath = dirPath;
        }

        public string SaveFile(string fullPath)
        {
            var id = GetUniqueId(fullPath);
            var path = Path.Combine(dirPath, id);
            File.Copy(fullPath, path);

            return id;
        }

        public string SaveStream(Stream stream)
        {
            var fullPath = Convert.ToString(Guid.NewGuid().ToByteArray()) + ".stream"; // simulate a fullpath
            var id = GetUniqueId(fullPath);
            var path = Path.Combine(dirPath, id);
            using (var fileStream = File.Create(path))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
            
            return id;
        }

        private string GetUniqueId(string fullPath)
        {
            var sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create(fullPath))
            {
                foreach (var t in md5.Hash)
                {
                    sb.Append(t.ToString("x2"));
                }
            }

            sb.Append("-").Append(DateTime.Now.ToString(CultureInfo.InvariantCulture).GetHashCode().ToString("x"));
            sb.Append(".").Append(Path.GetExtension(fullPath));

            return sb.ToString();
        }

        public Stream GetFile(string identifier)
        {
            var path = Path.Combine(dirPath, identifier);
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }
    }
}