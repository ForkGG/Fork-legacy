using System;
using System.IO;

namespace Fork.Logic.Model.BackupModels;

public class Backup
{
    public Backup(FileInfo file)
    {
        File = file;
        Name = Path.GetFileNameWithoutExtension(file.Name);
        Created = file.CreationTime;
    }

    public string Name { get; set; }
    public FileInfo File { get; set; }
    public DateTime Created { get; set; }

    public string Size
    {
        get
        {
            if (!File.Exists)
            {
                return "0 B";
            }

            long bytes = File.Length;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return bytes + " " + sizes[order];
        }
    }

    public override string ToString()
    {
        return Name;
    }
}
