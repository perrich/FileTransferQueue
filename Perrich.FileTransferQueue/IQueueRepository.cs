namespace Perrich.FileTransferQueue
{
    /// <summary>
    /// Repository used to persist a queue 
    /// </summary>
    public interface IQueueRepository
    {
        /// <summary>
        /// Save the provided queue
        /// </summary>
        /// <param name="queue"></param>
        void Save(FileTransferQueue queue);
        
        /// <summary>
        /// Load the queue
        /// </summary>
        /// <param name="name">The name of the queue</param>
        /// <returns></returns>
        FileTransferQueue Load(string name);
    }
}