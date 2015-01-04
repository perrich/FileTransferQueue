namespace Perrich.FtpQueue
{
    /// <summary>
    /// Repository used to persist a queue 
    /// </summary>
    public interface IFtpQueueRepository
    {
        /// <summary>
        /// Save the provided queue
        /// </summary>
        /// <param name="queue"></param>
        void Save(FtpQueue queue);
        
        /// <summary>
        /// Load the queue
        /// </summary>
        /// <param name="name">The name of the queue</param>
        /// <returns></returns>
        FtpQueue Load(string name);
    }
}