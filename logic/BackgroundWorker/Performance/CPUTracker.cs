using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Fork.ViewModel;

namespace Fork.Logic.BackgroundWorker.Performance
{
    public class CPUTracker
    {
        private bool interrupted;
        private readonly List<Thread> threads = new();

        public void TrackTotal(Process p, EntityViewModel viewModel)
        {
            PerformanceCounter cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };
            Thread t = new Thread(() =>
            {
                while (!interrupted && !p.HasExited)
                {
                    try
                    {
                        viewModel.CPUValueUpdate(cpuCounter.NextValue());
                    }
                    catch (Exception e)
                    {
                        break;
                    }

                    Thread.Sleep(500);
                }

                viewModel.CPUValueUpdate(0.0);
                viewModel.CPUValueUpdate(0.0);
                viewModel.CPUValueUpdate(0.0);
            }) {IsBackground = true};
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
                using (PerformanceCounter cnt = new PerformanceCounter("Process",
                    "ID Process", instance, true))
                {
                    int val = (int) cnt.RawValue;
                    if (val == processId) return instance;
                }

            return null;
        }

        public void StopThreads()
        {
            interrupted = true;
        }
    }
}