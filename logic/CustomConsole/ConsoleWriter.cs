using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Fork.Logic.Model;
using Fork.Logic.Model.ServerConsole;
using Fork.ViewModel;

namespace Fork.Logic.CustomConsole
{
    public class ConsoleWriter
    {
        private static Regex waterfallStarted = new Regex(@"\[([0-9]+:?)* INFO\]: Listening on /.*$");
        
        public delegate void ConsoleWriteEventHandler(string line, EntityViewModel source);
        public static event ConsoleWriteEventHandler ConsoleWriteLine;

        private delegate void CustomWriteEventHandler(string line, EntityViewModel target);
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
                        ConsoleWriteLine?.Invoke(line,viewModel);
                        
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
            }).Start();

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
                        ConsoleWriteLine?.Invoke(line,viewModel);

                        viewModel.AddToConsole(isSuccess
                            ? new ConsoleMessage(line, ConsoleMessage.MessageLevel.SUCCESS)
                            : new ConsoleMessage(line));
                    }
                }
            }).Start();
        }

        public static void Write(string line, EntityViewModel target)
        {
            target.AddToConsole(new ConsoleMessage(line));
        }
    }
}