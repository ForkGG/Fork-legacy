namespace Fork.Logic.Model
{
    public class AppSettings
    {
        public string ServerPath { get; set; } = App.ApplicationPath;
        public int MaxConsoleLines { get; set; } = 1000;
        public int MaxConsoleLinesPerSecond { get; set; } = 10;
        public bool SendTelemetry { get; set; } = false;
    }
}