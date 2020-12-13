using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Fork.Logic.ApplicationConsole;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Persistence;

namespace Fork
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
                    applicationPath = directoryInfo.FullName;
                    Console.WriteLine("Data directory of Fork is: "+applicationPath);
                }
                return applicationPath;
            }
        }

        public static string ServerPath => AppSettingsSerializer.Instance.AppSettings.ServerPath;

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            ApplicationManager.ConsoleWriter = new ConsoleWriter();
            Console.SetOut(ApplicationManager.ConsoleWriter);
            ErrorLogger logger = new ErrorLogger();
            base.OnStartup(e);
        }

        private void ExitApplication(object sender, ExitEventArgs exitEventArgs)
        {
            ApplicationManager.Instance.ExitApplication();
        }
    }
}
