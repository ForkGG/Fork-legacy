using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Fork.Logic.Model;
using Fork.Logic.Model.Automation;
using Fork.ViewModel;

namespace Fork.Logic.Manager
{
    public sealed class ServerAutomationManager
    {
        ///Singleton
        private static ServerAutomationManager instance;
        public static ServerAutomationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServerAutomationManager();
                }

                return instance;
            }
        }
        private ServerAutomationManager(){}
        
        private Dictionary<ServerViewModel,List<Timer>> Timers = new Dictionary<ServerViewModel, List<Timer>>();

        /// <summary>
        /// Dispose existing timers and set timer for the next automation event of a given server.
        /// This should be called every time the server status changes, or the settings change. 
        /// </summary>
        /// <param name="viewModel"></param>
        public void UpdateAutomation(ServerViewModel viewModel)
        {
            DisposeAutomation(viewModel);
            
            AutomationTime nextAutomationTime = GetNextAutomation(viewModel);
            if (nextAutomationTime != null)
            {
                RegisterRestart(viewModel, nextAutomationTime);
            }
            
            viewModel.SetAutomationTime(nextAutomationTime);
        }

        /// <summary>
        /// Disposes the timer for the next automation event of the given ServerViewModel
        /// </summary>
        /// <param name="viewModel"></param>
        public void DisposeAutomation(ServerViewModel viewModel)
        {
            if (!Timers.ContainsKey(viewModel))
            {
                return;
            }
            foreach (Timer timer in Timers[viewModel])
            {
                timer.Dispose();
            }
            Timers.Remove(viewModel);
        }

        /// <summary>
        /// Calculate the time until an automation event in milliseconds
        /// </summary>
        /// <param name="automationTime">Automation event</param>
        /// <returns>Time in milliseconds</returns>
        public double CalculateTime(AutomationTime automationTime)
        {
            DateTime nowTime = DateTime.Now;
            DateTime restartTime = new DateTime(nowTime.Year,nowTime.Month,nowTime.Day,automationTime.Time.Hours,automationTime.Time.Minutes,0);
            if (restartTime < nowTime)
            {
                restartTime = restartTime.AddDays(1);
            }
            return (restartTime - nowTime).TotalMilliseconds;
        }

        private AutomationTime GetNextAutomation(ServerViewModel server)
        {
            double nextTimer = double.MaxValue;
            AutomationTime result = null;
            List<AutomationTime> relevantTimes = GetRelevantTimes(server);
            foreach (AutomationTime automationTime in relevantTimes)
            {
                if (automationTime.Enabled)
                {
                    double t = CalculateTime(automationTime);
                    if (t < nextTimer)
                    {
                        nextTimer = t;
                        result = automationTime;
                    }
                }
            }
            return result;
        }

        private List<AutomationTime> GetRelevantTimes(ServerViewModel viewModel)
        {
            List<AutomationTime> result = new();
            if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                result.Add(viewModel.Server.AutoStart1);
                result.Add(viewModel.Server.AutoStart2);
            }
            else
            {
                result.Add(viewModel.Server.Restart1);
                result.Add(viewModel.Server.Restart2);
                result.Add(viewModel.Server.Restart3);
                result.Add(viewModel.Server.Restart4);
                result.Add(viewModel.Server.AutoStop1);
                result.Add(viewModel.Server.AutoStop2);
            }
            return result;
        }

        private void RegisterRestart(ServerViewModel server, AutomationTime time)
        {
            double restartTime = CalculateTime(time);
            Timer t = new Timer(restartTime);
            //t.AutoReset = cyclic;
            t.Elapsed += async (sender, e) => await Task.Factory.StartNew(()=>TimerElapsed(server,t,time));
            if (!Timers.ContainsKey(server))
            {
                Timers.Add(server,new List<Timer>());
            }
            Timers[server].Add(t);
            t.Start();
        }

        private void TimerElapsed(ServerViewModel server, Timer t, AutomationTime time)
        {
            if (time is RestartTime)
            {
                TimerElapsedRestart(server);
            }
            else if (time is StopTime)
            {
                TimerElapsedStop(server);
            } 
            else if (time is StartTime)
            {
                TimerElapsedStart(server);
            }
            if (!t.AutoReset)
            {
                t.Dispose();
                Timers[server].Remove(t);
            }
        }

        private void TimerElapsedRestart(ServerViewModel viewModel)
        {
            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                ServerManager.Instance.RestartServer(viewModel);
            }
        }

        private void TimerElapsedStart(ServerViewModel viewModel)
        {
            if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                Task.Run(() => ServerManager.Instance.StartServerAsync(viewModel));
            }
        }

        private void TimerElapsedStop(ServerViewModel viewModel)
        {
            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                ServerManager.Instance.StopServer(viewModel);
            }
        }
    }
}