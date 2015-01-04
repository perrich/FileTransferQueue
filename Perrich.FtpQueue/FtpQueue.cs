using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Perrich.FtpQueue
{
    public class FtpQueue
    {
        private readonly ISet<string> filenames = new HashSet<string>();
        private readonly Queue<FtpItem> queue = new Queue<FtpItem>();

        private readonly object syncObj = new object();

        /// <summary>
        /// Add an item into the queue
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(FtpItem item)
        {
            string filename = Path.GetFileName(item.DestPath);

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("Destination path should contain a filename");

            lock (syncObj)
            {
                queue.Enqueue(item);
                filenames.Add(filename);
            }
        }

        /// <summary>
        /// Get the first item in the queue
        /// </summary>
        /// <returns></returns>
        public FtpItem Dequeue()
        {
            FtpItem item = null;

            lock (syncObj)
            {
                try
                {
                    item = queue.Dequeue();
                    string filename = Path.GetFileName(item.DestPath);
                    filenames.Remove(filename);
                }
                catch (InvalidOperationException)
                {
                }
            }

            return item;
        }

        /// <summary>
        /// Is the queue contains the provided filename?
        /// </summary>
        /// <param name="destFilename"></param>
        /// <returns></returns>
        public bool Contains(string destFilename)
        {
            lock (syncObj)
            {
                return filenames.Contains(destFilename);
            }
        }

        /// <summary>
        /// Get all items and clear the queue
        /// </summary>
        /// <returns></returns>
        public IList<FtpItem> FlushItems()
        {
            IList<FtpItem> items;
            lock (syncObj)
            {
                items = queue.ToList();
                filenames.Clear();
                queue.Clear();
            }

            return items;
        }
    }
}