namespace Fork.Logic.Model.Automation
{
    public class StartTime : AutomationTime
    {
        public bool Enabled { get; set; }
        public SimpleTime Time { get; set; }
        
        public StartTime(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public StartTime(){}
    }
}