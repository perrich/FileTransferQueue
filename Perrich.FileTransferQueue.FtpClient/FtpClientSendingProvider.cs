using System;
using System.IO;

namespace Perrich.FileTransferQueue.FtpClient
{
    public class FtpClientSendingProvider : ISendingProvider
    {
        private readonly System.Net.FtpClient.FtpClient ftpClient;

        public FtpClientSendingProvider(System.Net.FtpClient.FtpClient ftpClient)
        {
            this.ftpClient = ftpClient;
        }

        public bool Send(Stream srcStream, string destPath)
        {
            try
            {
                using (var s = ftpClient.OpenWrite(destPath))
                {
                    try
                    {
                        srcStream.CopyTo(s);
                    }
                    finally
                    {
                        s.Close();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}