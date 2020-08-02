namespace Fork.Logic.Model
{
    public class AppSettings
    {
        public string ServerPath { get; set; } = App.ApplicationPath;
        public bool SendTelemetry { get; set; } = true;
    }
}