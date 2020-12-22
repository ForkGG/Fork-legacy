using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Fork.Annotations;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.Persistence
{
    public class AppSettingsSerializer : INotifyPropertyChanged
    {
        #region Singleton

        private static AppSettingsSerializer instance;
        public static AppSettingsSerializer Instance => instance ??= new AppSettingsSerializer();

        #endregion
        
        private AppSettings appSettings;
        public AppSettings AppSettings => appSettings ??= ReadAppSettings();

        private AppSettingsSerializer(){}

        public void SaveSettings()
        {
            OnPropertyChanged(nameof(AppSettings));
            WriteAppSettings(AppSettings);
        }

        public void ReadSettings()
        {
            appSettings = ReadAppSettings();
        }

        private AppSettings ReadAppSettings()
        {
            FileInfo settingsFile = new FileInfo(Path.Combine(App.ApplicationPath,"persistence","settings.json"));
            if (!settingsFile.Exists)
            {
                return WriteDefaultAppSettings();
            }

            try
            {
                string settingsJson = File.ReadAllText(settingsFile.FullName);
                return JsonConvert.DeserializeObject<AppSettings>(settingsJson);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                throw e;
            }
        }

        private void WriteAppSettings(AppSettings appSettings)
        {
            DirectoryInfo persistenceDir = new DirectoryInfo(Path.Combine(App.ApplicationPath, "persistence"));
            if (!persistenceDir.Exists)
            {
                persistenceDir.Create();
            }
            FileInfo settingsFile = new FileInfo(Path.Combine(persistenceDir.FullName, "settings.json"));
            if (!settingsFile.Exists)
            {
                var stream = settingsFile.Create();
                stream.Close();
            }

            try
            {
                string settingsJson = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
                File.WriteAllText(settingsFile.FullName,settingsJson);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                throw e;
            }
        }

        private AppSettings WriteDefaultAppSettings()
        {
            AppSettings defaultAppSettings = new AppSettings();
            WriteAppSettings(defaultAppSettings);
            return defaultAppSettings;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(nameof(AppSettingsSerializer), new PropertyChangedEventArgs(propertyName));
        }
    }
}