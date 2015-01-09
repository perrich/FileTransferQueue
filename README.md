FileTransferQueue
========

A local Queue to handle transfer errors. First need was to manage FTP transfert issue but the API is open and you can manage any transfert protocol and persistance solution.

The current version includes :
- a JSON config file to persist the queue
- a local file based system to persist file not already sent
- an FTP protocol using EdtFtpnet library


Usage:

```csharp
  var queueRepository = new FileFtpQueueRepository("your config file folder");
  var system = new LocalFileSystem("your queued files folder");
  var provider = new EdtftpnetSendingProvider(ftpConnexion);
  var manager = new FtpQueueManager("your queue name", queueRepository, system, provider);
  
  manager.InitAndApply(); // load configuration and try to send previously queued items
  
  foreach(var file in filesToSend)
  {
     manager.TryToSend(file, Path.GetFilename(file)); // try to send files in our current process
  }
  
  manager.ApplyAndSave(); // Try to resend not already sent files and save queue content (so, these files are not be sent but will be resend at the next restart)
```
