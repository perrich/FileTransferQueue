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
        /// Load into the provided queue
        /// </summary>
        /// <param name="queue"></param>
        void Load(FtpQueue queue);
    }
}