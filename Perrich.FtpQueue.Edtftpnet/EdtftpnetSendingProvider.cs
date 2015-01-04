using System;
using System.IO;
using EnterpriseDT.Net.Ftp;

namespace Perrich.FtpQueue.Edtftpnet
{
    public class EdtftpnetSendingProvider : ISendingProvider
    {
        private readonly SecureFTPConnection connexion;

        private string currentDirectory;

        public EdtftpnetSendingProvider(SecureFTPConnection connexion)
        {
            this.connexion = connexion;
        }

        public bool Send(Stream stream, string destPath)
        {
            try
            {
                var wantedDirectory = Path.GetDirectoryName(destPath);
                if (currentDirectory == null || wantedDirectory != currentDirectory)
                {
                    connexion.ChangeWorkingDirectory(wantedDirectory);
                    currentDirectory = wantedDirectory;
                }

                connexion.UploadStream(stream, Path.GetFileName(destPath), false);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}