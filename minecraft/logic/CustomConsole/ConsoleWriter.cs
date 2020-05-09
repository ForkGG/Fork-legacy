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
        private ServerViewModel viewModel;

        public ConsoleWriter(ServerViewModel viewModel, StreamReader stdOut, StreamReader errOut)
        {
            this.viewModel = viewModel;
            
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
                        //

                        viewModel.ConsoleOutList.Add(line);
                    }
                }
            }).Start();
        }

        public void Write(string line)
        {
            viewModel.ConsoleOutList.Add(line);
        }
    }
}