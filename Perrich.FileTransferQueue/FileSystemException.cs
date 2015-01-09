using System;

namespace Perrich.FileTransferQueue
{
    /// <summary>
    /// An exception occured in a file system action
    /// </summary>
    public class FileSystemException : Exception
    {
        /// <summary>
        /// Allowed Action type
        /// </summary>
        public enum ActionType
        {
            Read, 
            Write,
            Delete,
        }

        /// <summary>
        /// Current action
        /// </summary>
        public ActionType Type { get; private set; }

        public FileSystemException(ActionType type, string message)
            : base(message)
        {
            Type = type;
        }

        public FileSystemException(ActionType type, string message, Exception innerException)
            : base(message, innerException)
        {
            Type = type;
        }
    }
}
