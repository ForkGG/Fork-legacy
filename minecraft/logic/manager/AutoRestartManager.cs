using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.Logic.Manager
{
    public sealed class AutoRestartManager
    {
        ///Singleton
        private static AutoRestartManager instance;
        public static AutoRestartManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AutoRestartManager();
                }

                return instance;
            }
        }
        private AutoRestartManager(){}
        
        private Dictionary<ServerViewModel,List<Timer>> Timers = new Dictionary<ServerViewModel, List<Timer>>();

        public double RegisterRestart(ServerViewModel server)
        {
            double nextTimer = double.MaxValue;
            bool hasRestart = false;
            if (server.Server.Restart1.Enabled)
            {
                double t = CalculateTime(server.Server.Restart1);
                if (t<nextTimer)
                {
                    nextTimer = t;
                    hasRestart = true;
                }
            }
            if (server.Server.Restart2.Enabled)
            {
                double t = CalculateTime(server.Server.Restart2);
                if (t<nextTimer)
                {
                    nextTimer = t;
                    hasRestart = true;
                }            }
            if (server.Server.Restart3.Enabled)
            {
                double t = CalculateTime(server.Server.Restart3);
                if (t<nextTimer)
                {
                    nextTimer = t;
                    hasRestart = true;
                }            }
            if (server.Server.Restart4.Enabled)
            {
                double t = CalculateTime(server.Server.Restart4);
                if (t<nextTimer)
                {
                    nextTimer = t;
                    hasRestart = true;
                }            
            }
            if (hasRestart)
            {
                RegisterRestart(server, nextTimer);
                return nextTimer;
            }
            return -1d;
        }

        public void DisposeRestart(ServerViewModel server)
        {
            if (!Timers.ContainsKey(server))
            {
                return;
            }
            foreach (Timer timer in Timers[server])
            {
                timer.Dispose();
            }
            Timers.Remove(server);
        }

        private double CalculateTime(ServerRestart restart)
        {
            DateTime nowTime = DateTime.Now;
            DateTime restartTime = new DateTime(nowTime.Year,nowTime.Month,nowTime.Day,restart.Time.Hours,restart.Time.Minutes,0);
            if (restartTime < nowTime)
            {
                restartTime.AddDays(1);
            }
            return (restartTime - nowTime).TotalMilliseconds;
        }

        private void RegisterRestart(ServerViewModel server, double restartTime)
        {
            Timer t = new Timer(restartTime);
            //t.AutoReset = cyclic;
            t.Elapsed += async (sender, e) => await Task.Factory.StartNew(()=>TimerElapsed(server,t));
            if (!Timers.ContainsKey(server))
            {
                Timers.Add(server,new List<Timer>());
            }
            Timers[server].Add(t);
            t.Start();
        }

        private void TimerElapsed(ServerViewModel server, Timer t)
        {
            if (server.CurrentStatus == ServerStatus.RUNNING)
            {
                ServerManager.Instance.RestartServer(server);
            }
            if (!t.AutoReset)
            {
                t.Dispose();
                Timers[server].Remove(t);
            }
        }
    }
}