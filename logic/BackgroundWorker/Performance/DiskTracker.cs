using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Fork.ViewModel;

namespace Fork.Logic.BackgroundWorker.Performance
{
    public class DiskTracker
    {
        private bool interrupted = false;
        private List<Thread> threads = new List<Thread>();

        public void TrackTotal(Process p, EntityViewModel viewModel)
        {
            PerformanceCounter cpuCounter = new PerformanceCounter
            {
                CategoryName = "PhysicalDisk",
                CounterName = "% Disk Time",
                InstanceName = "_Total"
            };
            Thread t = new Thread(() =>
            {
                while (!interrupted && !p.HasExited)
                {
                    try
                    {
                        viewModel.DiskValueUpdate(cpuCounter.NextValue());
                    } catch(Exception e) { break; }
                    Thread.Sleep(500);
                }
                viewModel.DiskValueUpdate(0.0);
                viewModel.DiskValueUpdate(0.0);
                viewModel.DiskValueUpdate(0.0);
            });
            t.Start();
            t.IsBackground = true;
            threads.Add(t);
        }
        
        public void StopThreads()
        {
            interrupted = true;
        }
    }
}