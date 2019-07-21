using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using nihilus.Annotations;
using nihilus.Logic.BackgroundWorker.Performance;
using nihilus.Logic.CustomConsole;
using nihilus.Logic.Model;
using nihilus.Logic.Persistence;
using nihilus.View.Xaml.MainWindowFrames;

namespace nihilus.ViewModel
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        public static ServerViewModel HomeViewModel()
        {
            ServerViewModel homeViewModel = new ServerViewModel();

            return homeViewModel;
        }

        private string externalIP =  new WebClient().DownloadString("http://icanhazip.com").Trim();
        private bool isHome;

        private ChartValues<double> cpuTotal;
        private ChartValues<double> cpuServer;
        private CPUTracker cpuTracker;

        private ChartValues<double> memServer;
        private MEMTracker memTracker;

        public ConsoleReader ConsoleReader;
        public ObservableCollection<string> ConsoleOutList { get; }
        public ObservableCollection<Player> PlayerList { get; set; }
        public SeriesCollection CPUList { get; set; } = new SeriesCollection();
        public SeriesCollection MEMList { get; set; } = new SeriesCollection();
        public string ConsoleIn { get; set; } = "";
        public ServerStatus CurrentStatus { get; set; }
        public Server Server { get; set; }
        public string ServerTitle
        {
            get
            {
                if (isHome)
                {
                    return "Home";
                }
                return Server + Environment.NewLine + CurrentStatus;
            }
        }

        public string AddressInfo { get; set; }
        public Page ServerPage { get; set; }

        private ICommand readConsoleIn;
        public ICommand ReadConsoleIn
        {
            get
            {
                return readConsoleIn
                       ?? (readConsoleIn = new ActionCommand(() =>
                       {
                           ConsoleReader?.Read(ConsoleIn);
                           ConsoleIn = "";
                       }));
            }
        }

        public ServerViewModel(Server server)
        {
            Server = server;
            CurrentStatus = ServerStatus.STOPPED;
            ConsoleOutList = new ObservableCollection<string>();
            PlayerList = new ObservableCollection<Player>();
            ConsoleOutList.CollectionChanged += ConsoleOutChanged;
            UpdateAddressInfo();
            ServerPage = new ServerPage(this);
        }

        private ServerViewModel()
        {
            isHome = true;
            ServerPage = new StartPage();
        }

        private void UpdateAddressInfo()
        {
            AddressInfo = externalIP + ":" + Server.ServerSettings.ServerPort;
        }

        public void UpdateSettings()
        {
            UpdateAddressInfo();
            new Thread(() =>
            {
                new FileWriter().WriteServerSettings(Server.Name,Server.ServerSettings.SettingsDictionary);
            }).Start();
        }

        public void TrackPerformance(Process p)
        {
            // Track CPU usage
            cpuTracker?.StopThreads();
            
            cpuTracker = new CPUTracker();
            cpuTotal = new ChartValues<double>();
            cpuTracker.TrackTotal(p, cpuTotal,102);
            cpuServer = new ChartValues<double>();
            cpuTracker.TrackP(p,cpuServer, 102);

            CPUList = new SeriesCollection
            {
                new LineSeries
                {
                    Values = cpuTotal
                },
                new LineSeries
                {
                    Values = cpuServer
                }
            };
            cpuTotal.CollectionChanged += CPUListChanged;
            cpuServer.CollectionChanged += CPUListChanged;
            
            
            // Track memory usage
            memTracker?.StopThreads();
            
            memTracker = new MEMTracker();
            memServer = new ChartValues<double>();
            memTracker.TrackP(p,memServer,102);
            
            MEMList = new SeriesCollection
            {
                new LineSeries
                {
                    Values = memServer
                }
            };
            memServer.CollectionChanged += MEMListChanged;
        }

        private void ConsoleOutChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(ConsoleOutList));
        }

        private void CPUListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(CPUList));
        }


        private void MEMListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(MEMList));
        }

        
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        
        private class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}