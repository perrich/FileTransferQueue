using System.IO;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// Repository to save not sent file
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Save the file defined in the full path and return an unique identifier to retreive it
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns>A unique identifier</returns>
        string SaveFile(string fullPath);

        /// <summary>
        /// Save the content of the stream to a file
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        string SaveStream(Stream stream);

        /// <summary>
        /// Get a stream from an identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Stream GetFile(string identifier);
    }
}