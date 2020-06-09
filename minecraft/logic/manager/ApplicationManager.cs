using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using fork.Logic.ApplicationConsole;
using fork.Logic.Model;
using fork.ViewModel;

namespace fork.Logic.Manager
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
        public Dictionary<Entity, Process> ActiveEntities { get; set; } = new Dictionary<Entity, Process>();

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
            
        }
    }
}