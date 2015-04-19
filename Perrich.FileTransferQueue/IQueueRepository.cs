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
        /// <returns>The loaded queue or an empty queue in the other cases.</returns>
        FileTransferQueue Load(string name);

        /// <summary>
        /// Create a new empty queue using provided name
        /// </summary>
        /// <param name="name">The name of the queue</param>
        /// <returns>The new queue</returns>
        FileTransferQueue Create(string name);
    }
}