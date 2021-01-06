namespace Fork.Logic.Model.Automation
{
    public class RestartTime : AutomationTime
    {
        public bool Enabled { get; set; }
        public SimpleTime Time { get; set; }

        public RestartTime(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public RestartTime(){}
    }
}