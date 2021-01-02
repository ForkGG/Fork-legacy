using System.IO;

namespace Fork.Logic.Model
{
    public class AppSettings
    {
        public string ServerPath { get; set; } = Path.Combine(App.ApplicationPath,"servers");
        public int MaxConsoleLines { get; set; } = 1000;
        public int MaxConsoleLinesPerSecond { get; set; } = 10;
        public string DefaultJavaPath { get; set; } = "java.exe";
        public bool SendTelemetry { get; set; } = false;
    }
}