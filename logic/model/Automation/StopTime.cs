namespace Fork.Logic.Model.Automation
{
    public class StopTime : AutomationTime
    {
        public bool Enabled { get; set; }
        public SimpleTime Time { get; set; }
        
        public StopTime(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public StopTime(){}
    }
}