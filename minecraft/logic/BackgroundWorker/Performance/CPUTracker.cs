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

namespace nihilus.logic.Performance
{
    public class CPUTracker
    {
        private bool interrupted = false;
        private List<Thread> threads = new List<Thread>();

        public void TrackTotal(Process p, ChartValues<double> chartValues, int maxLength)
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
                    double value = cpuCounter.NextValue();
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            chartValues.Add(value);
                            if (chartValues.Count > maxLength)
                            {
                                chartValues.RemoveAt(0);
                            }
                        });
                    } catch(Exception e) { break; }
                    Thread.Sleep(1000);
                }
            });
            t.Start();
            threads.Add(t);
        }

        public void TrackP(Process p, ChartValues<double> chartValues, int maxLength)
        {
            string instanceName = GetProcessInstanceName(p.Id);
            PerformanceCounter cpuCounter = new PerformanceCounter
            {
                CategoryName = "Process",
                CounterName = "% Processor Time",
                InstanceName = instanceName
            };
            Thread t = new Thread(() =>
            {
                float processorCount = Environment.ProcessorCount;
                while (!interrupted && !p.HasExited)
                {
                    double value = cpuCounter.NextValue()/processorCount;
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            chartValues.Add(value);
                            if (chartValues.Count > maxLength)
                            {
                                chartValues.RemoveAt(0);
                            }
                        });
                    }
                    catch(Exception e){ break; }
                    Thread.Sleep(1000);
                }
            });
            t.Start();
            threads.Add(t);
        }

        public string GetProcessInstanceName(int processId)
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