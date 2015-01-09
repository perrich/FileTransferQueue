using System;
using System.IO;
using log4net;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Manage an FTP Queue
    /// </summary>
    public class FtpQueueManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FtpQueueManager).FullName);

        private FtpQueue ftpQueue;
        private readonly string queueName;
        private readonly IFtpQueueRepository queueRepository;
        private readonly IFileSystem system;
        private readonly ISendingProvider provider;

        public delegate void NotificationRaisedEventHandler(NotificationType type, FtpItem item);

        /// <summary>
        /// Allow to subscribe to notification
        /// </summary>
        public event NotificationRaisedEventHandler NotificationRaised;

        /// <summary>
        /// All notification type
        /// </summary>
        public enum NotificationType
        {
            Error,
            Warn,
        }

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
                    try
                    {
                        var stream = system.GetStream(item.Identifier);
                        var sended = TryToSend(stream, item.DestPath, item.Identifier);

                        if (sended)
                        {
                            system.Delete(item.Identifier);
                        }
                    }
                    catch (FileSystemException ex)
                    {
                        Log.Error(string.Format("Cannot send item (Id=\"{0}\", Dest=\"{1}\")", item.Identifier, item.DestPath), ex);
                        NotifyEvent(NotificationType.Error, item);
                    }
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
            var item = new FtpItem {Identifier = identifier, DestPath = destPath};
            ftpQueue.Enqueue(item);

            NotifyEvent(NotificationType.Warn, item);
            return false;
        }

        private void NotifyEvent(NotificationType notificationType, FtpItem item)
        {
            if (NotificationRaised != null)
                NotificationRaised(notificationType, item);
        }
    }
}
