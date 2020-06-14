using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using fork.Logic.ApplicationConsole;
using fork.Logic.Logging;
using fork.Logic.Manager;

namespace fork
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string applicationPath = null;
        public static string ApplicationPath
        {
            get
            {
                if (applicationPath == null)
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fork"));
                    //applicationPath = Assembly.GetExecutingAssembly().Location;
                    //FileInfo applicationExe = new FileInfo(applicationPath);
                    //applicationPath = applicationExe.Directory.FullName;
                    applicationPath = directoryInfo.FullName;
                    Console.WriteLine("Data directory of Fork is: "+applicationPath);
                }
                return applicationPath;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            ApplicationManager.ConsoleWriter.AppStarted();
            ErrorLogger logger = new ErrorLogger();
            base.OnStartup(e);
        }

        private void ExitApplication(object sender, ExitEventArgs exitEventArgs)
        {
            ApplicationManager.Instance.ExitApplication();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
        }
    }
}
