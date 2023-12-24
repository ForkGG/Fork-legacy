using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
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
#if DEBUG
        //Thanks to Alpha-c0d3r recommending this code in https://github.com/ForkGG/Fork/pull/21
        private static void InitializeConsole(bool alwaysCreateNewConsole = true)
        {
            bool consoleAttached = true;
            if (alwaysCreateNewConsole
                || (AttachConsole(ATTACH_PARRENT) == 0
                && Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED))
            {
                consoleAttached = AllocConsole() != 0;
            }

            if (consoleAttached)
            {
                InitializeOutStream();
                InitializeInStream();
            }
        }

        private static void InitializeOutStream()
        {
            var fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
            if (fs != null)
            {
                var writer = new StreamWriter(fs) { AutoFlush = true };
                Console.SetOut(writer);
                Console.SetError(writer);
            }
        }

        private static void InitializeInStream()
        {
            var fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
            if (fs != null)
            {
                Console.SetIn(new StreamReader(fs));
            }
        }

        private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
                                FileAccess dotNetFileAccess)
        {
            var file = new SafeFileHandle(CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
            if (!file.IsInvalid)
            {
                var fs = new FileStream(file, dotNetFileAccess);
                return fs;
            }
            return null;
        }

        #region Win API Functions and Constants
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll",
            EntryPoint = "AttachConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 AttachConsole(UInt32 dwProcessId);

        [DllImport("kernel32.dll",
            EntryPoint = "CreateFileW",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(
              string lpFileName,
              UInt32 dwDesiredAccess,
              UInt32 dwShareMode,
              IntPtr lpSecurityAttributes,
              UInt32 dwCreationDisposition,
              UInt32 dwFlagsAndAttributes,
              IntPtr hTemplateFile
            );

        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 FILE_SHARE_READ = 0x00000001;
        private const UInt32 FILE_SHARE_WRITE = 0x00000002;
        private const UInt32 OPEN_EXISTING = 0x00000003;
        private const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        private const UInt32 ERROR_ACCESS_DENIED = 5;

        private const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;

        #endregion
#endif
        
        
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
#if DEBUG
            InitializeConsole();
            Console.WriteLine("Detected Debug environment. Creating console window...");
#else
            Console.SetOut(ApplicationManager.ConsoleWriter);
#endif
            ErrorLogger logger = new ErrorLogger();
            base.OnStartup(e);
        }

        private void ExitApplication(object sender, ExitEventArgs exitEventArgs)
        {
            ApplicationManager.Instance.ExitApplication();
        }
    }
}
