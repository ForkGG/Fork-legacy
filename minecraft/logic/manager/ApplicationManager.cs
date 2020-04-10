using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using nihilus.Logic.ApplicationConsole;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.Logic.Manager
{
    public sealed class ApplicationManager
    {
        public static ConsoleWriter ConsoleWriter = new ConsoleWriter();
        private static ApplicationManager instance = null;
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
                        }
                    }
                }
                return instance;
            }
        }
        private ApplicationManager()
        {
        }
        
        
        public MainViewModel MainViewModel { get; } = new MainViewModel();
        public ConsoleViewModel ConsoleViewModel { get; } = new ConsoleViewModel();
        public Dictionary<Server, Process> ActiveServers { get; set; } = new Dictionary<Server, Process>();

        public void ExitApplication()
        {
            List<Process> serversToEnd = new List<Process>(ActiveServers.Values);
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

                        }
                    }
                }
            }
            
        }
    }
}