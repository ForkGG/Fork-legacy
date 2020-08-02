using System;
using System.Data;
using System.Windows;
using Fork.Logic.Persistence;

namespace Fork.Logic.Logging
{
    public class ErrorLogger
    {
        private static FileWriter fileWriter = new FileWriter();
        
        public ErrorLogger()
        {
            /*AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                fileWriter.AppendToErrorLog("["+DateTime.Now+"] [FirstChanceException] " +eventArgs.Exception.StackTrace+"\n");
            };*/

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Exception e = eventArgs.ExceptionObject as Exception;
                fileWriter.AppendToErrorLog("["+DateTime.Now+"] [UnhandledException] "+e?.GetType()+"\n" +e?.StackTrace+"\n");
            };
        }

        public static void Append(Exception e)
        {
            fileWriter.AppendToErrorLog("["+DateTime.Now+"] [HandledException] " +e?.StackTrace+"\n");
        }
    }
}