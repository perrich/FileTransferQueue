using System;
using System.IO;
using log4net;

namespace Perrich.FileTransferQueue
{
    /// <summary>
    /// Manage file sending using a file transfer queue when a reject occurs
    /// </summary>
    public class FileTransferQueueManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FileTransferQueueManager).FullName);

        private FileTransferQueue fileQueue;
        private readonly string queueName;
        private readonly IQueueRepository queueRepository;
        private readonly IFileSystem system;
        private readonly ISendingProvider provider;

        public delegate void NotificationRaisedEventHandler(NotificationType type, FileItem item);

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
            Success,
        }

        public FileTransferQueueManager(string queueName, IQueueRepository queueRepository, IFileSystem system, ISendingProvider provider)
        {
            this.queueName = queueName;
            fileQueue = new FileTransferQueue(queueName);
            this.queueRepository = queueRepository;
            this.system = system;
            this.provider = provider;
        }

        /// <summary>
        /// Initialize the manager
        /// </summary>
        public void Init()
        {
            fileQueue = queueRepository.Load(queueName);
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
            foreach (var item in fileQueue.FlushItems())
            {
                try
                {
                    ApplyItem(item);
                }
                catch (FileSystemException ex)
                {
                    Log.Error(string.Format("Cannot send item ({0}): issue in file system.", item), ex);
                    NotifyEvent(NotificationType.Error, item);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Cannot send item ({0}): not managed issue.", item), ex);
                    NotifyEvent(NotificationType.Error, item);
                }
            }
        }

        private void ApplyItem(FileItem item)
        {
            if (item.SrcPath != null)
            {
                TryToSend(item.SrcPath, item.DestPath);
            }
            else if (item.Identifier != null)
            {
                var stream = system.GetStream(item.Identifier);
                var sended = TryToSend(stream, item.DestPath, item.Identifier);

                if (sended)
                {
                    system.Delete(item.Identifier);
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
            queueRepository.Save(fileQueue);
        }

        /// <summary>
        /// Try to send a file
        /// </summary>
        /// <param name="srcPath">The local path</param>
        /// <param name="destPath">The remote path</param>
        /// <returns>True if file is sent</returns>
        /// <exception cref="ArgumentException">Parameter issue</exception>
        /// <exception cref="FileSystemException">File system issue</exception>
        public bool TryToSend(string srcPath, string destPath)
        {
            if (string.IsNullOrEmpty(srcPath))
                throw new ArgumentException("The source path should not be null or empty");
            if (string.IsNullOrEmpty(destPath))
                throw new ArgumentException("The destination path should not be null or empty");

            using (var stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            {
                if (provider.Send(stream, destPath))
                {
                    NotifyEvent(NotificationType.Success, new FileItem { SrcPath = srcPath, DestPath = destPath });
                    return true;
                }
            }

            var item = new FileItem { SrcPath = srcPath, DestPath = destPath };
            fileQueue.Enqueue(item);

            NotifyEvent(NotificationType.Warn, item);
            return false;
        }

        /// <summary>
        /// Try to send a stream
        /// </summary>
        /// <param name="stream">The stream to save</param>
        /// <param name="destPath">The remote path</param>
        /// <returns>True if stream is sent</returns>
        /// <exception cref="ArgumentException">Parameter issue</exception>
        /// <exception cref="FileSystemException">File system issue</exception>
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
            if (provider.Send(stream, destPath))
            {
                NotifyEvent(NotificationType.Success, new FileItem { Identifier = identifier, DestPath = destPath });
                return true;
            }

            Log.WarnFormat("Cannot send stream to {0}. Add it to the queue.", destPath);

            identifier = identifier ?? system.SaveStream(stream);
            stream.Close();

            var item = new FileItem {Identifier = identifier, DestPath = destPath};
            fileQueue.Enqueue(item);

            NotifyEvent(NotificationType.Warn, item);
            return false;
        }

        private void NotifyEvent(NotificationType notificationType, FileItem item)
        {
            if (NotificationRaised != null)
                NotificationRaised(notificationType, item);
        }
    }
}
