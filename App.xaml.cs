using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Fork.Logic.ApplicationConsole;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Persistence;
using Microsoft.Win32.SafeHandles;

namespace Fork;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static string applicationPath;

    public static string ApplicationPath
    {
        get
        {
            if (applicationPath == null)
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fork"));
                applicationPath = directoryInfo.FullName;
                Console.WriteLine("Data directory of Fork is: " + applicationPath);
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
        ErrorLogger logger = new();
        base.OnStartup(e);
    }

    private void ExitApplication(object sender, ExitEventArgs exitEventArgs)
    {
        ApplicationManager.Instance.ExitApplication();
    }
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
        FileStream fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
        if (fs != null)
        {
            StreamWriter writer = new StreamWriter(fs) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
        }
    }

    private static void InitializeInStream()
    {
        FileStream fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
        if (fs != null)
        {
            Console.SetIn(new StreamReader(fs));
        }
    }

    private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
        FileAccess dotNetFileAccess)
    {
        SafeFileHandle file =
            new SafeFileHandle(
                CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL,
                    IntPtr.Zero), true);
        if (!file.IsInvalid)
        {
            FileStream fs = new FileStream(file, dotNetFileAccess);
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
    private static extern uint AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll",
        EntryPoint = "CreateFileW",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr CreateFileW(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    private const uint GENERIC_WRITE = 0x40000000;
    private const uint GENERIC_READ = 0x80000000;
    private const uint FILE_SHARE_READ = 0x00000001;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint OPEN_EXISTING = 0x00000003;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    private const uint ERROR_ACCESS_DENIED = 5;

    private const uint ATTACH_PARRENT = 0xFFFFFFFF;

    #endregion

#endif
}