namespace Fork.Logic.Model.Automation;

public interface AutomationTime
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public SimpleTime Time { get; set; }
}