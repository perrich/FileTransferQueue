using System;
using System.IO;
using EnterpriseDT.Net.Ftp;

namespace Perrich.FileTransferQueue.EdtFilenet
{
    public class EdtFilenetSendingProvider : ISendingProvider
    {
        private readonly SecureFTPConnection connexion;

        private string currentDirectory;

        private readonly bool enablechangeDirectory;

        public EdtFilenetSendingProvider(SecureFTPConnection connexion, bool enablechangeDirectory)
        {
            this.connexion = connexion;
            this.enablechangeDirectory = enablechangeDirectory;
        }

        public bool Send(Stream stream, string destPath)
        {
            try
            {
                if (enablechangeDirectory)
                {
                    var wantedDirectory = Path.GetDirectoryName(destPath);
                    if ((currentDirectory == null || wantedDirectory != currentDirectory) && wantedDirectory != null)
                    {
                        connexion.ChangeWorkingDirectory(wantedDirectory);
                        currentDirectory = wantedDirectory;
                    }
                    connexion.UploadStream(stream, Path.GetFileName(destPath), false);
                }
                else
                {
                    connexion.UploadStream(stream, destPath, false);
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