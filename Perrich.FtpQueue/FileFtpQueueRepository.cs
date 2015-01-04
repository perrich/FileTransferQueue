using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Save the queue in a local file (using a JSON serialization).
    /// Filename is named as "xxx.queue" with xxx as queue name.
    /// </summary>
    public class FileFtpQueueRepository : IFtpQueueRepository
    {
        private readonly string directory;

        private static readonly Regex RejectedFilenameCharRegexRegex = new Regex("[?|%|*|:|\\||\"|/|\\]", RegexOptions.Compiled);
        
        /// <summary>
        /// Define a repository using the provided directory
        /// </summary>
        /// <param name="directory"></param>
        public FileFtpQueueRepository(string directory)
        {
            this.directory = directory;
        }

        public void Save(FtpQueue queue)
        {
            var str = JsonConvert.SerializeObject(queue.FlushItems());

            using (var writer = new StreamWriter(GetFullPath(queue.Name)))
            {
                writer.Write(str);
            }
        }

        public FtpQueue Load(string name)
        {
            var queue = new FtpQueue { Name = name };
            string str;

            using (var reader = new StreamReader(GetFullPath(name)))
            {
                str = reader.ReadToEnd();
            }

            var list = JsonConvert.DeserializeObject<List<FtpItem>>(str);
            foreach (var item in list)
            {
                queue.Enqueue(item);
            }

            return queue;
        }

        private string GetFullPath(string name)
        {
            return Path.Combine(directory, RejectedFilenameCharRegexRegex.Replace(name, "") + ".queue");
        }
    }
}