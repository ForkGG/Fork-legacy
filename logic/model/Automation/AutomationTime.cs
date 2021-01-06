namespace Fork.Logic.Model.Automation
{
    public interface AutomationTime
    {
        public bool Enabled { get; set; }
        public SimpleTime Time { get; set; } 
    }
}