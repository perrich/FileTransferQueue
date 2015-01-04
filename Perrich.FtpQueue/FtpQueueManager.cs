using System.IO;

namespace Perrich.FtpQueue
{
    public class FtpQueueManager
    {
        private readonly FtpQueue ftpQueue;
        private readonly IFtpQueueRepository queueRepository;
        private readonly IFileRepository repository;
        private readonly ISendingProvider provider;

        public FtpQueueManager(FtpQueue ftpQueue, IFtpQueueRepository queueRepository, IFileRepository repository, ISendingProvider provider)
        {
            this.ftpQueue = ftpQueue;
            this.queueRepository = queueRepository;
            this.repository = repository;
            this.provider = provider;
        }

        public void Init()
        {
            queueRepository.Load(ftpQueue);
        }

        public void InitAndApply()
        {
            Init();
            ApplyQueue();
        }

        public void ApplyQueue()
        {
            foreach (var item in ftpQueue.FlushItems())
            {
                if (item.SrcPath == null)
                {
                    TryToSend(item.SrcPath, item.DestPath);
                }
                else
                {
                    TryToSend(repository.GetFile(item.Identifier), item.DestPath, item.Identifier);
                }
            }
        }

        public void ApplyAndSave()
        {
            ApplyQueue();
            Save();
        }

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
            var stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
            if (provider.Send(stream, destPath)) return true;

            ftpQueue.Enqueue(new FtpItem { SrcPath = srcPath, DestPath = destPath });
            return false;
        }

        /// <summary>
        /// Try to send a file
        /// </summary>
        /// <param name="stream">The stream to save</param>
        /// <param name="destPath">The remote path</param>
        /// <returns></returns>
        public bool TryToSend(Stream stream, string destPath)
        {
            return TryToSend(stream, destPath, null);
        }

        private bool TryToSend(Stream stream, string destPath, string identifier)
        {
            if (provider.Send(stream, destPath)) return true;

            identifier = identifier ?? repository.SaveStream(stream);
            ftpQueue.Enqueue(new FtpItem { Identifier = identifier, DestPath = destPath });
            return false;
        }
    }
}
