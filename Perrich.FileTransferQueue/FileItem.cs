
namespace Perrich.FileTransferQueue
{
    /// <summary>
    /// Define a File to be transfered
    /// </summary>
    public class FileItem
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

        public override string ToString()
        {
            if (Identifier != null)
                return string.Format("Identifier: {0}, DestPath: {1}", Identifier, DestPath);
            else
                return string.Format("SrcPath: {0}, DestPath: {1}", SrcPath, DestPath);
        }
    }
}
