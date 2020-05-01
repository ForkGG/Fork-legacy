using System;
using System.Data;
using System.Windows;
using nihilus.Logic.Persistence;

namespace nihilus.Logic.Logging
{
    public class ErrorLogger
    {
        private FileWriter fileWriter = new FileWriter();
        
        public ErrorLogger()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                fileWriter.AppendToErrorLog("["+DateTime.Now+"] " +eventArgs.Exception.StackTrace+"\n");
            };
        }
    }
}