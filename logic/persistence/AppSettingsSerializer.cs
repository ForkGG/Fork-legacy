using System;
using System.IO;
using System.Text.Json.Serialization;
using fork.Logic.Logging;
using fork.Logic.Manager;
using fork.Logic.Model;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace fork.Logic.Persistence
{
    public class AppSettingsSerializer
    {
        public static AppSettings ReadAppSettings()
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

        public static void WriteAppSettings(AppSettings appSettings)
        {
            DirectoryInfo persistenceDir = new DirectoryInfo(Path.Combine(App.ApplicationPath, "persistence"));
            if (!persistenceDir.Exists)
            {
                persistenceDir.Create();
            }
            FileInfo settingsFile = new FileInfo(Path.Combine(persistenceDir.FullName, "settings.json"));
            if (!settingsFile.Exists)
            {
                settingsFile.Create();
            }

            try
            {
                string settingsJson = JsonConvert.SerializeObject(appSettings);
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