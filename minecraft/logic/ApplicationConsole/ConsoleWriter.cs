using System.Collections.Generic;
using System.IO;
using System.Text;
using nihilus.Logic.Manager;

namespace nihilus.Logic.ApplicationConsole
{
    public class ConsoleWriter : TextWriter
    {
        private List<string> preCachedList = new List<string>();
        private bool appStarted = false;
        
        public override Encoding Encoding => Encoding.ASCII;

        public override void WriteLine(string value)
        {
            if (appStarted)
            {
                ApplicationManager.Instance.ConsoleViewModel.WriteLine(value);
            }
            else
            {
                preCachedList.Add(value);
            }
        }

        public void AppStarted()
        {
            appStarted = true;
            foreach (string s in preCachedList)
            {
                WriteLine(s);
            }
        }
    }
}