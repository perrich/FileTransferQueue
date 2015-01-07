using System;
using System.IO;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Manage an FTP Queue
    /// </summary>
    public class FtpQueueManager
    {
        private FtpQueue ftpQueue;
        private readonly string queueName;
        private readonly IFtpQueueRepository queueRepository;
        private readonly IFileSystem system;
        private readonly ISendingProvider provider;

        public FtpQueueManager(string queueName, IFtpQueueRepository queueRepository, IFileSystem system, ISendingProvider provider)
        {
            this.queueName = queueName;
            ftpQueue = new FtpQueue(queueName);
            this.queueRepository = queueRepository;
            this.system = system;
            this.provider = provider;
        }

        /// <summary>
        /// Initialize the manager
        /// </summary>
        public void Init()
        {
            ftpQueue = queueRepository.Load(queueName);
        }

        /// <summary>
        /// Initialize the manager and try to send the currently queued items
        /// </summary>
        public void InitAndApply()
        {
            Init();
            ApplyQueue();
        }

        /// <summary>
        /// Try to send currently queued items
        /// </summary>
        public void ApplyQueue()
        {
            foreach (var item in ftpQueue.FlushItems())
            {
                if (item.SrcPath != null)
                {
                    TryToSend(item.SrcPath, item.DestPath);
                }
                else if (item.Identifier != null)
                {
                    TryToSend(system.GetStream(item.Identifier), item.DestPath, item.Identifier);
                }
            }
        }

        /// <summary>
        /// Try to send currently queued items and save not sent items
        /// </summary>
        public void ApplyAndSave()
        {
            ApplyQueue();
            Save();
        }

        /// <summary>
        /// Save the queued items
        /// </summary>
        public void Save()
        {
            queueRepository.Save(ftpQueue);
        }

        /// <summary>
        /// Try to send a file
        /// </summary>
        /// <param name="srcPath">The local path</param>
        /// <param name="destPath">The remote path</param>
        /// <returns></returns>
        public bool TryToSend(string srcPath, string destPath)
        {
            if (string.IsNullOrEmpty(srcPath))
                throw new ArgumentException("The source path should not be null or empty");
            if (string.IsNullOrEmpty(destPath))
                throw new ArgumentException("The destination path should not be null or empty");

            using (var stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            {
                if (provider.Send(stream, destPath)) return true;
            }

            ftpQueue.Enqueue(new FtpItem { SrcPath = srcPath, DestPath = destPath });
            return false;
        }

        /// <summary>
        /// Try to send a stream
        /// </summary>
        /// <param name="stream">The stream to save</param>
        /// <param name="destPath">The remote path</param>
        /// <returns></returns>
        public bool TryToSend(Stream stream, string destPath)
        {
            if (stream == null)
                throw new ArgumentException("The stream should never be null");
            if (string.IsNullOrEmpty(destPath))
                throw new ArgumentException("The destination path should not be null or empty");

            return TryToSend(stream, destPath, null);
        }

        private bool TryToSend(Stream stream, string destPath, string identifier)
        {
            if (provider.Send(stream, destPath)) return true;

            stream.Close();

            identifier = identifier ?? system.SaveStream(stream);
            ftpQueue.Enqueue(new FtpItem { Identifier = identifier, DestPath = destPath });
            return false;
        }
    }
}
