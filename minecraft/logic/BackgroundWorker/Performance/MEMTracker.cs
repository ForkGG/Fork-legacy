using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using LiveCharts;
using System.Management;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.Logic.BackgroundWorker.Performance
{
    public class MEMTracker
    {
        private bool interrupted = false;
        private List<Thread> threads = new List<Thread>();

        public void TrackP(Process p, ServerViewModel viewModel)
        {
            string instanceName = GetProcessInstanceName(p.Id);
            PerformanceCounter memCounter = new PerformanceCounter
            {
                CategoryName = "Process",
                CounterName = "Working Set - Private",
                InstanceName = instanceName
            };
            Thread t = new Thread(() =>
            {
                while (!interrupted && !p.HasExited)
                {
                    try
                    {
                        viewModel.MemValueUpdate(memCounter.NextValue()/(1024*1024));
                    }
                    catch(Exception e){ break; }
                    Thread.Sleep(500);
                }
                viewModel.MemValueUpdate(0.0);
            });
            t.Start();
            threads.Add(t);
        }

        private string GetProcessInstanceName(int processId)
        {
            var process = Process.GetProcessById(processId);
            string processName = Path.GetFileNameWithoutExtension(process.ProcessName);

            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
            string[] instances = cat.GetInstanceNames()
                .Where(inst => inst.StartsWith(processName))
                .ToArray();

            foreach (string instance in instances)
            {
                using (PerformanceCounter cnt = new PerformanceCounter("Process",
                    "ID Process", instance, true))
                {
                    int val = (int)cnt.RawValue;
                    if (val == processId)
                    {
                        return instance;
                    }
                }
            }
            return null;
        }

        public void StopThreads()
        {
            interrupted = true;
        }
    }
}