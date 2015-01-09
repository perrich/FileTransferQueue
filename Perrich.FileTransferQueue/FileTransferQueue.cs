using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Perrich.FileTransferQueue
{
    /// <summary>
    /// Queue which contains ordered files to transfer
    /// </summary>
    public class FileTransferQueue
    {
        private readonly ISet<string> filenames = new HashSet<string>();
        private readonly Queue<FileItem> queue = new Queue<FileItem>();

        private readonly object syncObj = new object();

        public string Name { get; private set; }

        /// <summary>
        /// Create an File Queue
        /// </summary>
        /// <param name="name"></param>
        public FileTransferQueue(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Add an item into the queue
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(FileItem item)
        {
            var filename = Path.GetFileName(item.DestPath);

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
        public FileItem Dequeue()
        {
            FileItem item = null;

            lock (syncObj)
            {
                try
                {
                    item = queue.Dequeue();
                    var filename = Path.GetFileName(item.DestPath);
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
        public IList<FileItem> FlushItems()
        {
            IList<FileItem> items;
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