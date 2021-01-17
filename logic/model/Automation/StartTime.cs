namespace Fork.Logic.Model.Automation
{
    public class StartTime : AutomationTime
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public virtual SimpleTime Time { get; set; }
        
        public StartTime(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public StartTime(){}
    }
}