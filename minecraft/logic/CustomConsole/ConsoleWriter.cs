using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using fork.Logic.Model;
using fork.ViewModel;

namespace fork.Logic.CustomConsole
{
    public class ConsoleWriter
    {
        public delegate void ConsoleWriteEventHandler(string line, ServerViewModel source);
        public static event ConsoleWriteEventHandler ConsoleWriteLine;


        private delegate void CustomWriteEventHandler(string line, ServerViewModel target);
        private static event CustomWriteEventHandler CustomWriteLine;
        
        public static void RegisterApplication(ServerViewModel viewModel, StreamReader stdOut, StreamReader errOut)
        {
            new Thread(() =>
            {
                while (!stdOut.EndOfStream)
                {
                    string line = stdOut.ReadLine();
                    if (line != null)
                    {
                        if (line.Contains("For help, type \"help\""))
                        {
                            viewModel.CurrentStatus = ServerStatus.RUNNING;
                        }
                        ConsoleWriteLine?.Invoke(line,viewModel);
                        viewModel.RoleInputHandler(line);
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
                        viewModel.RoleInputHandler(line);
                        ConsoleWriteLine?.Invoke(line,viewModel);
                        viewModel.ConsoleOutList.Add(line);
                    }
                }
            }).Start();
        }

        public static void Write(string line, ServerViewModel target)
        {
            target.ConsoleOutList.Add(line);
        }
    }
}