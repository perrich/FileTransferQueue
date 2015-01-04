
namespace Perrich.FtpQueue
{
    /// <summary>
    /// Define a File to be sent by FTP
    /// </summary>
    public class FtpItem
    {
        /// <summary>
        /// The source identifier in the repository
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// The full source path
        /// </summary>
        public string SrcPath { get; set; }


        /// <summary>
        /// The full destination path
        /// </summary>
        public string DestPath { get; set; }
    }
}
