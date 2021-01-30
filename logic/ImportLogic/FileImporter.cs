using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Fork.Logic.ImportLogic
{
    public class FileImporter
    {
        public delegate void CopyProgressChangedEventHandler(
            object sender,
            CopyProgressChangedEventArgs e);

        public CopyProgressChangedEventHandler CopyProgressChanged;

        private CopyProgressChangedEventArgs eventArgs;

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = false,
            List<string> ignoredFiles = null)
        {
            if (ignoredFiles == null) ignoredFiles = new List<string>();
            int filesToCopy = FilesToCopy(sourceDirName, copySubDirs, ignoredFiles);
            eventArgs = new CopyProgressChangedEventArgs(0, null, 0, filesToCopy);
            DirectoryCopyInternal(sourceDirName, destDirName, copySubDirs, ignoredFiles);
        }

        public void DirectoryMove(string sourceDirName, string destDirName, bool moveSubDirs = false)
        {
            int filesToCopy = FilesToCopy(sourceDirName, moveSubDirs, new List<string>());
            eventArgs = new CopyProgressChangedEventArgs(0, null, 0, filesToCopy);
            DirectoryMoveInternal(sourceDirName, destDirName, moveSubDirs);
            Directory.Delete(sourceDirName, true);
        }

        private void DirectoryCopyInternal(string sourceDirName, string destDirName, bool copySubDirs,
            List<string> ignoredFiles)
        {
            List<string> ignoredFilesInternal = new List<string>();
            foreach (string ignoredFile in ignoredFiles) ignoredFilesInternal.Add(ignoredFile.ToLower());
            ignoredFiles = ignoredFilesInternal;
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (ignoredFiles.Contains(file.Name.ToLower())) continue;
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
                eventArgs.FilesCopied++;
                OnCopyProgressChanged(eventArgs);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopyInternal(subdir.FullName, temppath, copySubDirs, ignoredFiles);
                }
        }

        private void DirectoryMoveInternal(string sourceDirName, string destDirName, bool moveSubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.MoveTo(temppath, true);
                eventArgs.FilesCopied++;
                OnCopyProgressChanged(eventArgs);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (moveSubDirs)
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryMoveInternal(subdir.FullName, temppath, moveSubDirs);
                }
        }

        private int FilesToCopy(string sourceDirName, bool copySubDirs, List<string> ignoredFiles)
        {
            int result = 0;

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (ignoredFiles.Contains(file.Name)) continue;

                result++;
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                    result += FilesToCopy(subdir.FullName, copySubDirs, ignoredFiles);
            }

            return result;
        }

        protected virtual void OnCopyProgressChanged(CopyProgressChangedEventArgs e)
        {
            if (CopyProgressChanged == null)
                return;
            CopyProgressChanged(this, e);
        }

        public class CopyProgressChangedEventArgs : ProgressChangedEventArgs
        {
            internal CopyProgressChangedEventArgs(int progressPercentage, object userState, int filesCopied,
                int filesToCopy) : base(progressPercentage, userState)
            {
                FilesCopied = filesCopied;
                FilesToCopy = filesToCopy;
            }

            public int FilesCopied { get; set; }
            public int FilesToCopy { get; set; }
        }
    }
}