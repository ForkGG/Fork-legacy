using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using fork.Logic.Manager;
using fork.Logic.Persistence;

namespace fork.Logic.ApplicationConsole
{
    public class ConsoleWriter : TextWriter
    {
        private List<string> preCachedList = new List<string>();
        private bool appStarted = false;
        private FileInfo logFile;
        
        public override Encoding Encoding => Encoding.ASCII;

        public ConsoleWriter() { }

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
                if (!new DirectoryInfo(Path.Combine(App.ApplicationPath,"logs")).Exists)
                {
                    Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "logs"));
                }
                var r = logFile.Create();
                r.Close();
            }
            
            appStarted = true;
            foreach (string s in preCachedList)
            {
                WriteLine(s);
            }
        }
    }
}