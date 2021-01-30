using System.IO;
using Fork.ViewModel;

namespace Fork.Logic.CustomConsole
{
    public class ConsoleReader
    {
        private StreamWriter stdIn;
        
        public ConsoleReader(StreamWriter stdIn)
        {
            this.stdIn = stdIn;
        }

        public void Read(string line, EntityViewModel source)
        {
            stdIn.WriteLine(line);
            ConsoleWriter.Write(line, source);
        }
    }
}