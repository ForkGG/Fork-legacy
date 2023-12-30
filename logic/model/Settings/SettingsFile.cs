using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Persistence;
using Fork.ViewModel;
using ICSharpCode.AvalonEdit.Utils;
using FileReader = Fork.Logic.Persistence.FileReader;

namespace Fork.Logic.Model.Settings
{
    public class SettingsFile
    {
        public enum SettingsType
        {
            Vanilla, Bungee, Undefined
        }
        public class TextReadUpdatedEventArgs
        {
            public string NewText { get; }

            public TextReadUpdatedEventArgs(string newText)
            {
                NewText = newText;
            }
        }
        public delegate void HandleTextReadUpdate(object sender, TextReadUpdatedEventArgs eventArgs);
        public event HandleTextReadUpdate TextReadUpdateEvent;
        
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
        private bool textChanged = false;
        private string text;
        
        public FileInfo FileInfo { get; set; }
        public int NameID { get; }

        public string Text
        {
            get => text;
            set
            {
                textChanged = true;
                text = value;
            }
        }

        public SettingsType Type { get; }
        

        //Constructor for Settings without file
        public SettingsFile(string name)
        {
            Type = SettingsType.Undefined;
            NameID = GetNameID(name);
            FileInfo = new FileInfo(Path.Combine(App.ApplicationPath,"persistence","entities.json"));
        }
        public SettingsFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            ReadText();
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

            NameID = GetNameID(FileInfo.Name);
        }


        public async Task ReadText()
        {
            await Task.Run(async () =>
            {
                while (!FileReader.IsFileReadable(FileInfo))
                {
                    await Task.Delay(500);
                    if (ApplicationManager.Instance.HasExited || !FileInfo.Exists)
                    {
                        return;
                    }
                }

                await semaphoreSlim.WaitAsync();
                try
                {
                    await using FileStream fileStream = new FileStream(FileInfo.FullName, FileMode.OpenOrCreate,
                        FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader streamReader = new StreamReader(fileStream);
                    Text = await streamReader.ReadToEndAsync();
                    TextReadUpdateEvent?.Invoke(this, new TextReadUpdatedEventArgs(Text));
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            });
        }

        public async Task SaveText()
        {
            if (textChanged)
            {
                textChanged = false;
                await Task.Run(async () =>
                {
                    while (!FileWriter.IsFileWritable(FileInfo))
                    {
                        await Task.Delay(500);
                        if (ApplicationManager.Instance.HasExited)
                        {
                            return;
                        }
                    }

                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        await using FileStream fileStream = new FileStream(FileInfo.FullName, FileMode.Create,
                            FileAccess.Write, FileShare.Read);
                        await using StreamWriter streamWriter = new StreamWriter(fileStream);
                        await streamWriter.WriteAsync(Text);
                    }
                    catch (Exception e)
                    {
                        ErrorLogger.Append(e);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                });
            }
        }

        private int GetNameID(string filename)
        {
            switch (filename)
            {
                case "Settings":
                    return -1;
                case "server.properties":
                    return 0;
                case "config.yml":
                    return 0;
                case "paper.yml":
                    return 1;
                case "purpur.yml":
                    return 1;
                case "waterfall.yml":
                    return 1;
                case "spigot.yml":
                    return 2;
                case "bukkit.yml":
                    return 3;
                case "permissions.yml":
                    return 11;
                case "help.yml":
                    return 30;
                default:
                    return 10;
            }
        }
        
        
        public override string ToString()
        {
            return FileInfo.Name;
        }

        protected bool Equals(SettingsFile other)
        {
            return Equals(FileInfo, other.FileInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SettingsFile) obj);
        }

        public override int GetHashCode()
        {
            return (FileInfo != null ? FileInfo.GetHashCode() : 0);
        }
    }
}