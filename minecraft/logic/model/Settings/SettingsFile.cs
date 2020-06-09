using System.IO;
using System.Text;

namespace fork.Logic.Model.Settings
{
    public class SettingsFile
    {
        public enum SettingsType
        {
            Vanilla, Bungee, Undefined
        }
        
        public FileInfo FileInfo { get; set; }
        public string Text { get; set; }

        public string Name => ToString();
        public SettingsType Type { get; }

        
        public SettingsFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            Text = File.ReadAllText(FileInfo.FullName);
            switch (fileInfo.Name)
            {
                case "server.properties":
                    Type = SettingsType.Vanilla;
                    break;
                case "config.yml":
                    Type = SettingsType.Bungee;
                    break;
                default:
                    Type = SettingsType.Undefined;
                    break;
            }
        }


        public void ResetText()
        {
            Text = File.ReadAllText(FileInfo.FullName);
        }

        public void SaveText()
        {
            File.WriteAllText(FileInfo.FullName,Text);
        }
        public override string ToString()
        {
            return FileInfo.Name;
        }
    }
}