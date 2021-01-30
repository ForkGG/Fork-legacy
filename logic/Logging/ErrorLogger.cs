using System;
using Fork.Logic.Persistence;

namespace Fork.Logic.Logging
{
    public class ErrorLogger
    {
        private static readonly FileWriter fileWriter = new();

        public ErrorLogger()
        {
            /*AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                fileWriter.AppendToErrorLog("["+DateTime.Now+"] [FirstChanceException] " +eventArgs.Exception.StackTrace+"\n");
            };*/

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Exception e = eventArgs.ExceptionObject as Exception;
                string errorMessage = e?.GetType() + "\n" + e?.Message + "\n" + e?.StackTrace;
#if DEBUG
                Console.WriteLine(errorMessage);
#endif
                fileWriter.AppendToErrorLog("[" + DateTime.Now + "] [UnhandledException] " + errorMessage + "\n");
            };
        }

        public static void Append(Exception e)
        {
            fileWriter.AppendToErrorLog("[" + DateTime.Now + "] [HandledException] " + e?.GetType() + "\n" +
                                        e?.Message + "\n" + e?.StackTrace + "\n");
        }
    }
}