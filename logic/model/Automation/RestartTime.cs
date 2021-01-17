namespace Fork.Logic.Model.Automation
{
    public class RestartTime : AutomationTime
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public virtual SimpleTime Time { get; set; }

        public RestartTime(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public RestartTime(){}
    }
}