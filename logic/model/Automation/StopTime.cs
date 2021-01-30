namespace Fork.Logic.Model.Automation
{
    public class StopTime : AutomationTime
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public virtual SimpleTime Time { get; set; }
        
        public StopTime(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public StopTime(){}
    }
}