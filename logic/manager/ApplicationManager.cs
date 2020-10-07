using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Threading;
using Fork.Logic.ApplicationConsole;
using Fork.Logic.Controller;
using Fork.Logic.Model;
using Fork.Logic.Model.APIModels;
using Fork.Logic.Persistence;
using Fork.Logic.WebRequesters;
using Fork.Properties;
using Fork.ViewModel;

namespace Fork.Logic.Manager
{
    public sealed class ApplicationManager
    {
        private static string userAgent;

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

        public static ConsoleWriter ConsoleWriter;
        private static ApplicationManager instance = null;
        public static bool Initialized { get; private set; } = false;
        public delegate void OnApplicationInitialized();
        public static event OnApplicationInitialized ApplicationInitialized;

        //Lock to ensure Singleton pattern
        private static object myLock = new object();
        
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
        private ApplicationManager()
        {
            ResourceManager rm = Resources.ResourceManager;
            CurrentForkVersion = new ForkVersion
            {
                Major = int.Parse(rm.GetString("VersionMajor")),
                Minor = int.Parse(rm.GetString("VersionMinor")),
                Patch = int.Parse(rm.GetString("VersionPatch"))
            };
        }

        private AppSettings appSettings;
        
        
        public MainViewModel MainViewModel { get; } = 
            new MainViewModel();
        public ConsoleViewModel ConsoleViewModel { get; } = 
            new ConsoleViewModel();
        public Dictionary<Entity, Process> ActiveEntities { get; } = new Dictionary<Entity, Process>();
        public List<SettingsReader> SettingsReaders { get; } = new List<SettingsReader>();
        public bool HasExited { get; set; } = false;
        public ForkVersion CurrentForkVersion { get; }

        public void ExitApplication()
        {
            List<Process> serversToEnd = new List<Process>(ActiveEntities.Values);
            foreach (Process process in serversToEnd)
            {
                if (process != null)
                {
                    process.StandardInput.WriteLine("stop");
                    if (!process.WaitForExit(5000))
                    {
                        try
                        {
                            process.Kill();
                        } catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            foreach (SettingsReader settingsReader in SettingsReaders)
            {
                settingsReader.Dispose();
            }

            HasExited = true;
        }
    }
}