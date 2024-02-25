using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Fork.Logic.Model;
using Fork.Logic.Model.ServerConsole;
using Fork.ViewModel;

namespace Fork.Logic.CustomConsole;

public class ConsoleWriter
{
    public delegate void ConsoleWriteEventHandler(string line, EntityViewModel source);

    private static readonly Regex waterfallStarted = new(@"\[([0-9]+:?)* INFO\]: Listening on /.*$");
    public static event ConsoleWriteEventHandler ConsoleWriteLine;
    private static event CustomWriteEventHandler CustomWriteLine;

    public static void RegisterApplication(EntityViewModel viewModel, StreamReader stdOut, StreamReader errOut)
    {
        new Thread(() =>
        {
            while (!stdOut.EndOfStream)
            {
                string line = stdOut.ReadLine();
                if (line != null)
                {
                    ConsoleWriteLine?.Invoke(line, viewModel);

                    if (line.Contains(@"WARN Advanced terminal features are not available in this environment"))
                    {
                        continue;
                    }

                    //bool used to generate green success message in console
                    bool isSuccess = false;
                    if (viewModel is ServerViewModel serverViewModel)
                    {
                        if (line.Contains("For help, type \"help\""))
                        {
                            serverViewModel.CurrentStatus = ServerStatus.RUNNING;
                            isSuccess = true;
                        }

                        serverViewModel.RoleInputHandler(line);
                    }

                    if (viewModel is NetworkViewModel networkViewModel)
                    {
                        if (waterfallStarted.Match(line).Success)
                        {
                            networkViewModel.CurrentStatus = ServerStatus.RUNNING;
                            isSuccess = true;
                        }
                    }

                    viewModel.AddToConsole(isSuccess
                        ? new ConsoleMessage(line, ConsoleMessage.MessageLevel.SUCCESS)
                        : new ConsoleMessage(line));
                }
            }
        }) { IsBackground = true }.Start();

        new Thread(() =>
        {
            while (!errOut.EndOfStream)
            {
                string line = errOut.ReadLine();
                if (line != null)
                {
                    bool isSuccess = false;
                    // For early minecraft versions
                    if (line.Contains("For help, type \"help\""))
                    {
                        viewModel.CurrentStatus = ServerStatus.RUNNING;
                        isSuccess = true;
                    }

                    if (viewModel is ServerViewModel serverViewModel)
                    {
                        serverViewModel.RoleInputHandler(line);
                    }

                    ConsoleWriteLine?.Invoke(line, viewModel);

                    viewModel.AddToConsole(isSuccess
                        ? new ConsoleMessage(line, ConsoleMessage.MessageLevel.SUCCESS)
                        : new ConsoleMessage(line));
                }
            }
        }) { IsBackground = true }.Start();
    }

    public static void Write(string line, EntityViewModel target)
    {
        target.AddToConsole(new ConsoleMessage(line));
    }

    public static void Write(ConsoleMessage message, EntityViewModel target)
    {
        target.AddToConsole(message);
    }

    private delegate void CustomWriteEventHandler(string line, EntityViewModel target);
}