namespace Fork.Logic.Model.WebSocketModels;

public class WsRestart
{
    public bool Enabled { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
}