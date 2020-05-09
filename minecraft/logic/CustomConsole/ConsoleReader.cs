using System.IO;

namespace fork.Logic.CustomConsole
{
    public class ConsoleReader
    {
        private StreamWriter stdIn;
        private ConsoleWriter writer;
        
        public ConsoleReader(StreamWriter stdIn, ConsoleWriter writer)
        {
            this.stdIn = stdIn;
            this.writer = writer;
        }

        public void Read(string line)
        {
            stdIn.WriteLine(line);
            writer.Write(line);
        }
    }
}