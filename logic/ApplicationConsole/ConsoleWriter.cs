using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fork.Logic.Manager;

namespace Fork.Logic.ApplicationConsole
{
    public class ConsoleWriter : TextWriter
    {
        private bool appStarted;
        private FileInfo logFile;
        private readonly List<string> preCachedList = new();

        public override Encoding Encoding => Encoding.ASCII;

        public override void WriteLine(string value)
        {
            if (appStarted)
            {
                DateTime now = DateTime.Now;
                value = "[" + now + "] " + value;
                ApplicationManager.Instance.ConsoleViewModel.WriteLine(value);
                using (StreamWriter sw = logFile.AppendText())
                {
                    sw.WriteLine(value);
                }
            }
            else
            {
                preCachedList.Add(value);
            }
        }

        public void AppStarted()
        {
            logFile = new FileInfo(Path.Combine(App.ApplicationPath, "logs", "consoleLog.txt"));
            if (!logFile.Exists)
            {
                if (!new DirectoryInfo(Path.Combine(App.ApplicationPath, "logs")).Exists)
                    Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "logs"));
                var r = logFile.Create();
                r.Close();
            }

            appStarted = true;
            foreach (string s in preCachedList) WriteLine(s);
        }
    }
}