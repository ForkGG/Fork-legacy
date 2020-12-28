using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Fork.Logic.Logging;
using Fork.ViewModel;

namespace Fork.Logic.BackgroundWorker.Performance
{
    public class MEMTracker
    {
        private bool interrupted = false;
        private List<Thread> threads = new List<Thread>();

        public void TrackP(Process p, EntityViewModel viewModel)
        {
            //TODO this is not working with Java 15 bc of subprocess spawned
            Thread t = new Thread(() =>
            {
                while (!interrupted && !p.HasExited)
                {
                    try
                    {
                        p.Refresh();
                        viewModel.MemValueUpdate(p.WorkingSet64/(1024d*1024d));
                        Thread.Sleep(500);
                    }
                    catch { break; }
                }
                viewModel.MemValueUpdate(0.0);
                viewModel.MemValueUpdate(0.0);
                viewModel.MemValueUpdate(0.0);
            }){IsBackground = true};
            t.Start();
            threads.Add(t);
        }

        private string GetProcessInstanceName(int processId)
        {
            try
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
                        int val = (int) cnt.RawValue;
                        if (val == processId)
                        {
                            return instance;
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return null;
            }
        }

        public void StopThreads()
        {
            interrupted = true;
        }
    }
}