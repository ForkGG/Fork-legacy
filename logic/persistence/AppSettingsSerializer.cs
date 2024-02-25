using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Fork.Annotations;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Utils;
using Newtonsoft.Json;

namespace Fork.Logic.Persistence;

public class AppSettingsSerializer : INotifyPropertyChanged
{
    private AppSettings appSettings;

    private AppSettingsSerializer()
    {
    }

    public AppSettings AppSettings
    {
        get
        {
            if (appSettings == null)
            {
                ReadSettings();
            }

            return appSettings;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void SaveSettings()
    {
        OnPropertyChanged(nameof(AppSettings));
        WriteAppSettings(AppSettings);
    }

    public void ReadSettings()
    {
        appSettings = ReadAppSettings();
        if (string.IsNullOrEmpty(appSettings.DiscordBotToken))
        {
            appSettings.DiscordBotToken = TokenUtils.GenerateDiscordToken();
            WriteAppSettings(appSettings);
        }
    }

    private AppSettings ReadAppSettings()
    {
        FileInfo settingsFile = new(Path.Combine(App.ApplicationPath, "persistence", "settings.json"));
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
        DirectoryInfo persistenceDir = new(Path.Combine(App.ApplicationPath, "persistence"));
        if (!persistenceDir.Exists)
        {
            persistenceDir.Create();
        }

        FileInfo settingsFile = new(Path.Combine(persistenceDir.FullName, "settings.json"));
        if (!settingsFile.Exists)
        {
            FileStream stream = settingsFile.Create();
            stream.Close();
        }

        try
        {
            string settingsJson = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
            File.WriteAllText(settingsFile.FullName, settingsJson);
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            throw e;
        }
    }

    private AppSettings WriteDefaultAppSettings()
    {
        AppSettings defaultAppSettings = new();
        WriteAppSettings(defaultAppSettings);
        return defaultAppSettings;
    }

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(nameof(AppSettingsSerializer), new PropertyChangedEventArgs(propertyName));
    }

    #region Singleton

    private static AppSettingsSerializer instance;
    public static AppSettingsSerializer Instance => instance ??= new AppSettingsSerializer();

    #endregion
}