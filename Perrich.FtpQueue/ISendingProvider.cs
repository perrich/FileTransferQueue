using System.IO;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Provide a way to send a file from a local path to a remote path
    /// </summary>
    public interface ISendingProvider
    {
        /// <summary>
        /// Send a file. 
        /// Do not throw exception
        /// </summary>
        /// <param name="srcStream">The stream to send</param>
        /// <param name="destPath">The remote path</param>
        /// <returns></returns>
        bool Send(Stream srcStream, string destPath);
    }
}
