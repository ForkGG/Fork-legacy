using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using nihilus.Logic.ApplicationConsole;
using nihilus.Logic.Manager;

namespace nihilus
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
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nihilus"));
                    //applicationPath = Assembly.GetExecutingAssembly().Location;
                    //FileInfo applicationExe = new FileInfo(applicationPath);
                    //applicationPath = applicationExe.Directory.FullName;
                    applicationPath = directoryInfo.FullName;
                    Console.WriteLine("Data directory of Nihilus is: "+applicationPath);
                }
                return applicationPath;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationManager.ConsoleWriter.AppStarted();
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
