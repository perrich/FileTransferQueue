using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Save the queue in a local file
    /// </summary>
    public class FileFtpQueueRepository : IFtpQueueRepository
    {
        private readonly string fullPath;
        
        /// <summary>
        /// Define a repository using the file provided with its full path
        /// </summary>
        /// <param name="fullPath"></param>
        public FileFtpQueueRepository(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public void Save(FtpQueue queue)
        {
            var str = JsonConvert.SerializeObject(queue.FlushItems());

            using (var writer = new StreamWriter(fullPath))
            {
                writer.Write(str);
            }
        }

        public void Load(FtpQueue queue)
        {
            string str;

            using (var reader = new StreamReader(fullPath))
            {
                str = reader.ReadToEnd();
            }

            var list = JsonConvert.DeserializeObject<List<FtpItem>>(str);
            foreach (var item in list)
            {
                queue.Enqueue(item);
            }
        }
    }
}