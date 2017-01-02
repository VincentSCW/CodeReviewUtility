using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utility.Library.Misc
{
    public sealed class FileWatcher
    {
        public event EventHandler<FileSystemEventArgs> FileChanged;
        public event EventHandler<RenamedEventArgs> FileRenamed;

        private FileSystemWatcher wachter;

        public FileWatcher(string folder) 
        {
            //Create a Filesystem watcher object
            this.wachter = new FileSystemWatcher(folder, "*.*");
            // Allow the watcher to raise events when any modifications are made to watched folder
            this.wachter.EnableRaisingEvents = true;
            
            this.wachter.IncludeSubdirectories = true;
            // Notice all sorts of changes. Other types of changes are changed, deleted, renamed, created
         //   this.wachter.WaitForChanged(WatcherChangeTypes.All);
            
            this.wachter.Changed += (o, e) => this.RaiseOnFileChanged(e);
            this.wachter.Created += (o, e) => this.RaiseOnFileChanged(e);
            this.wachter.Deleted += (o, e) => this.RaiseOnFileChanged(e);
            this.wachter.Renamed += (o, e) => this.RaiseOnFileRenamed(e);
        }

        public void Stop()
        {
            this.StopWatching();
            this.wachter.Dispose();
        }

        public void StopWatching()
        {
            this.wachter.EnableRaisingEvents = false;
        }

        public void StartWatching()
        {
            this.wachter.EnableRaisingEvents = true;
        }

        private void RaiseOnFileChanged(FileSystemEventArgs e)
        {
            if (this.FileChanged != null)
                this.FileChanged(this, e);
        }

        private void RaiseOnFileRenamed(RenamedEventArgs e)
        {
            if (this.FileRenamed != null)
                this.FileRenamed(this, e);
        }
    }
}
