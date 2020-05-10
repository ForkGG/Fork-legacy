using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace fork.Logic.ImportLogic
{
    public class FileImporter
    {
        public CopyProgressChangedEventHandler CopyProgressChanged;

        private CopyProgressChangedEventArgs eventArgs;

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = false,
            List<string> ignoredFiles = null)
        {
            if (ignoredFiles == null)
            {
                ignoredFiles = new List<string>();
            }
            int filesToCopy = FilesToCopy(sourceDirName, copySubDirs, ignoredFiles);
            eventArgs = new CopyProgressChangedEventArgs(0,null,0,filesToCopy);
            DirectoryCopyInternal(sourceDirName,destDirName,copySubDirs,ignoredFiles);
        }
        
        private void DirectoryCopyInternal(string sourceDirName, string destDirName, bool copySubDirs, 
            List<string> ignoredFiles)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (ignoredFiles.Contains(file.Name))
                {
                    continue;
                }
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
                eventArgs.FilesCopied++;
                OnCopyProgressChanged(eventArgs);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopyInternal(subdir.FullName, temppath, copySubDirs, ignoredFiles);
                }
            }
        }

        private int FilesToCopy(string sourceDirName, bool copySubDirs, List<string> ignoredFiles)
        {
            int result = 0;
            
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (ignoredFiles.Contains(file.Name))
                {
                    continue;
                }

                result++;
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    result += FilesToCopy(subdir.FullName, copySubDirs, ignoredFiles);
                }
            }

            return result;
        }
        
        protected virtual void OnCopyProgressChanged(CopyProgressChangedEventArgs e)
        {
            if (this.CopyProgressChanged == null)
                return;
            this.CopyProgressChanged((object) this, e);
        }
        
        public class CopyProgressChangedEventArgs : ProgressChangedEventArgs
        {
            public int FilesCopied { get; set; }
            public int FilesToCopy { get; set; }
            internal CopyProgressChangedEventArgs(int progressPercentage, object userState, int filesCopied, int filesToCopy) : base(progressPercentage, userState)
            {
                FilesCopied = filesCopied;
                FilesToCopy = filesToCopy;
            }
        }

        public delegate void CopyProgressChangedEventHandler(
            object sender,
            CopyProgressChangedEventArgs e);
    }
}