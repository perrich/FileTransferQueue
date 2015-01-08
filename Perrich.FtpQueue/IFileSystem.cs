using System.IO;

namespace Perrich.FtpQueue
{
    /// <summary>
    /// System to save not sent files
    /// </summary>
    public interface IFileSystem
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
        /// <returns>A unique identifier</returns>
        string SaveStream(Stream stream);

        /// <summary>
        /// Get a stream from an identifier
        /// </summary>
        /// <param name="identifier">The unique identifier</param>
        /// <returns></returns>
        Stream GetStream(string identifier);

        /// <summary>
        /// Delete entry saved with the provided identifier
        /// </summary>
        /// <param name="identifier">The unique identifier</param>
        /// <returns></returns>
        void Delete(string identifier);
    }
}