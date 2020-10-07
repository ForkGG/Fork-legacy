using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Fork.Logic.Model;
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
                        if (viewModel is ServerViewModel serverViewModel)
                        {
                            if (line.Contains("For help, type \"help\""))
                            {
                                serverViewModel.CurrentStatus = ServerStatus.RUNNING;
                            }
                            serverViewModel.RoleInputHandler(line);
                        }

                        if (viewModel is NetworkViewModel networkViewModel)
                        {
                            if (waterfallStarted.Match(line).Success)
                            {
                                networkViewModel.CurrentStatus = ServerStatus.RUNNING;
                            }
                        }
                        viewModel.ConsoleOutList.Add(line);
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
                        // For early minecraft versions
                        if (line.Contains("For help, type \"help\""))
                        {
                            viewModel.CurrentStatus = ServerStatus.RUNNING;
                        }

                        if (viewModel is ServerViewModel serverViewModel)
                        {
                            serverViewModel.RoleInputHandler(line);
                        }
                        ConsoleWriteLine?.Invoke(line,viewModel);
                        viewModel.ConsoleOutList.Add(line);
                    }
                }
            }).Start();
        }

        public static void Write(string line, EntityViewModel target)
        {
            target.ConsoleOutList.Add(line);
        }
    }
}