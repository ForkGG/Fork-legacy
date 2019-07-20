using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using nihilus.logic.model;
using nihilus.logic.ViewModel;

namespace nihilus.logic.Console
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