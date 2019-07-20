using System;
using System.Collections.Generic;
using System.Diagnostics;
using nihilus.logic.model;
using nihilus.logic.ViewModel;

namespace nihilus.logic
{
    public sealed class ApplicationManager
    {
        private static ApplicationManager instance = null;

        public static ApplicationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApplicationManager();
                }

                return instance;
            }
        }
        private ApplicationManager()
        {
            MainViewModel = new MainViewModel();
        }
        
        
        public MainViewModel MainViewModel { get; }
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