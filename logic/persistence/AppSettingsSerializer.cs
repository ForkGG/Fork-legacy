using System;
using System.IO;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.Persistence
{
    public class AppSettingsSerializer
    {
        private static AppSettings appSettings;
        public static AppSettings AppSettings => appSettings ??= ReadAppSettings();

        public static void SaveSettings()
        {
            WriteAppSettings(AppSettings);
        }

        public static void ReadSettings()
        {
            appSettings = ReadAppSettings();
        }

        private static AppSettings ReadAppSettings()
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

        private static void WriteAppSettings(AppSettings appSettings)
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

        private static AppSettings WriteDefaultAppSettings()
        {
            AppSettings defaultAppSettings = new AppSettings();
            WriteAppSettings(defaultAppSettings);
            return defaultAppSettings;
        }
    }
}