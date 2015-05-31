FileTransferQueue
========

A persistant queue implementation to handle transfer errors. First need was to manage FTP transfer issues but it is possible to implement any solution. You can manage any transfer protocol (FTP, HTTP, message queue, ...) and persistence solution (file, database, message queue, ...).

The current version includes :
- a JSON configuration file to persist the queue
- a local file based system to persist file not already sent
- FTP protocols using EdtFtpnet and System.Net.FtpClient libraries


Usage:

```csharp
  ...
  var queueRepository = new LocalQueueRepository("your config file folder");
  var system = new LocalFileSystem("your queued files folder");
  var provider = new FtpClientSendingProvider(ftpConnexion);
  var manager = new FileTransferQueueManager("your queue name", queueRepository, system, provider);
  manager.NotificationRaised += manager_NotificationRaised;
  manager.InitAndApply(); // load configuration and try to send previously queued items
  
  foreach(var file in filesToSend)
  {
     manager.TryToSend(file, Path.GetFilename(file)); // try to send files in our current process
  }
  
  manager.ApplyAndSave(); // Try to resend not already sent files and save queue content (so, these files are not be sent but will be resend at the next restart)
  ...
  void manager_NotificationRaised(FileTransferQueueManager.NotificationType type, FileItem item)
  {
	// manage success/error notification
  }
 ```

Used libraries:
---
- NUnit for unit tests
- FakeItEasy for unit tests
- Log4net for log 
- Newtonsoft.Json (used in JSON file config)


License:
---
Copyright 2015 PERRICHOT Florian

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
