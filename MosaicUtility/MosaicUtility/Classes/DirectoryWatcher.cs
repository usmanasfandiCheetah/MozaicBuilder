using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosaicUtility.Classes
{ 
    public class DirectoryWatcher
    {
        public event EventHandler OnNewFile;
        public bool IsRunning { get; set; }
        FileSystemWatcher watcher;
        DateTime fsLastRaised;
        string directory = "";
        int totalFiles = 0;
        string latestFile = "";
        List<string> processedFiles;


        public DirectoryWatcher() { }

        public DirectoryWatcher(string path)
        {
            directory = path;
            processedFiles = new List<string>();
            watcher = new FileSystemWatcher(directory);
            watcher.Created += Watcher_Created;
        }

        public string getNewFile()
        {
            return latestFile;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            //if (DateTime.Now.Subtract(fsLastRaised).TotalMilliseconds > 1000)
            //{
                //to get the newly created file name and extension and also the name of the event occured in the watching folder
                latestFile = Path.Combine(directory,e.Name);
                //FileInfo createdFile = new FileInfo(CreatedFileName);
                //string extension = createdFile.Extension;
                //string eventoccured = e.ChangeType.ToString();
                //latestFile = createdFile.FullName;
                //to note the time of event occured
                fsLastRaised = DateTime.Now;
                //Delay is given to the thread for avoiding same process to be repeated
                System.Threading.Thread.Sleep(100);
                OnNewFile(latestFile, null);
            //}
        }

        public void Start()
        {
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            IsRunning = true;

        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            IsRunning = false;
        }

    }
}
