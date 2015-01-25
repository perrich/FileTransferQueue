namespace Perrich.FileTransferQueue
{
    /// <summary>
    /// Define a File to be transfered.
    /// It allows a src folder or an identifier but not both in the same time.
    /// </summary>
    public class FileItem
    {
        private string identifier;
        private string srcPath;

        /// <summary>
        /// The source identifier in the repository
        /// </summary>
        public string Identifier
        {
            get { return identifier; }

            set
            {
                srcPath = null;
                identifier = value;
            }
        }

        /// <summary>
        /// The full source path
        /// </summary>
        public string SrcPath
        {
            get { return srcPath; }
            set
            {
                identifier = null;
                srcPath = value;
            }
        }

        /// <summary>
        /// The full destination path
        /// </summary>
        public string DestPath { get; set; }

        public override string ToString()
        {
            if (identifier != null)
                return string.Format("Identifier: {0}, DestPath: {1}", identifier, DestPath);
            return string.Format("SrcPath: {0}, DestPath: {1}", srcPath, DestPath);
        }
    }
}