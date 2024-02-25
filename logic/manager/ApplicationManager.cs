using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using System.Threading.Tasks;
using Fork.Logic.ApplicationConsole;
using Fork.Logic.Model;
using Fork.Logic.Model.APIModels;
using Fork.Logic.Model.EventArgs;
using Fork.Logic.Persistence;
using Fork.Logic.Utils;
using Fork.Logic.WebRequesters;
using Fork.Properties;
using Fork.ViewModel;

namespace Fork.Logic.Manager;

public sealed class ApplicationManager
{
    public delegate void OnApplicationInitialized();

    public delegate void PlayerEventHandler(object sender, PlayerEventArgs e);

    public delegate void ServerListEventHandler(object sender, EventArgs e);

    private static string userAgent;

    public static ConsoleWriter ConsoleWriter;
    private static ApplicationManager instance;
    private static WebSocketHandler webSocketHandler;

    //Lock to ensure Singleton pattern
    private static readonly object myLock = new();

    private ApplicationManager()
    {
        ResourceManager rm = Resources.ResourceManager;
        CurrentForkVersion = new ForkVersion
        {
            Major = int.Parse(rm.GetString("VersionMajor")),
            Minor = int.Parse(rm.GetString("VersionMinor")),
            Patch = int.Parse(rm.GetString("VersionPatch")),
            Beta = int.Parse(rm.GetString("VersionBeta"))
        };
        DiscordRichPresenceUtils.SetupRichPresence();
    }

    public static string UserAgent
    {
        get
        {
            if (userAgent == null)
            {
                ResourceManager rm = Resources.ResourceManager;
                userAgent = rm.GetString("UserAgent") + " - v" + rm.GetString("VersionMajor") +
                            "." + rm.GetString("VersionMinor") + "." + rm.GetString("VersionPatch");
            }

            return userAgent;
        }
    }

    public static bool Initialized { get; private set; }

    public static ApplicationManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (myLock)
                {
                    if (instance == null)
                    {
                        instance = new ApplicationManager();
                        ConsoleWriter.AppStarted();
                        Initialized = true;
                        ApplicationInitialized?.Invoke();
                    }
                }
            }

            return instance;
        }
    }

    public MainViewModel MainViewModel { get; } = new();

    public Dictionary<Entity, Process> ActiveEntities { get; } = new();
    public List<SettingsReader> SettingsReaders { get; } = new();
    public bool HasExited { get; set; }
    public ForkVersion CurrentForkVersion { get; }
    public static event OnApplicationInitialized ApplicationInitialized;

    public static void StartDiscordWebSocket()
    {
        webSocketHandler?.Dispose();
        webSocketHandler = new WebSocketHandler();
        Task.Run(() => webSocketHandler.SetupDiscordWebSocket());
    }

    public static void StopDiscordWebSocket()
    {
        webSocketHandler?.Dispose();
        webSocketHandler = null;
    }

    public event PlayerEventHandler PlayerEvent;
    public event ServerListEventHandler ServerListEvent;

    public void TriggerPlayerEvent(object sender, PlayerEventArgs e)
    {
        PlayerEvent?.Invoke(sender, e);
    }

    public void TriggerServerListEvent(object sender, EventArgs e)
    {
        ServerListEvent?.Invoke(sender, e);
    }

    public void ExitApplication()
    {
        List<Process> serversToEnd = new(ActiveEntities.Values);
        foreach (Process process in serversToEnd)
            if (process != null)
            {
                process.StandardInput.WriteLine("stop");
                if (!process.WaitForExit(5000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

        //if(serversToEnd.Count > 0)
        //{
        //   TriggerServerListEvent(this, EventArgs.Empty);
        //}
        StopDiscordWebSocket();

        foreach (SettingsReader settingsReader in SettingsReaders) settingsReader.Dispose();

        HasExited = true;
    }
}