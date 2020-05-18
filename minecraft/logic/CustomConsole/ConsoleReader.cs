using System.IO;
using fork.ViewModel;

namespace fork.Logic.CustomConsole
{
    public class ConsoleReader
    {
        private StreamWriter stdIn;
        
        public ConsoleReader(StreamWriter stdIn)
        {
            this.stdIn = stdIn;
        }

        public void Read(string line, ServerViewModel source)
        {
            stdIn.WriteLine(line);
            ConsoleWriter.Write(line, source);
        }
    }
}