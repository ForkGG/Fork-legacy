using System.Collections.Generic;

namespace Fork.Logic.Model.WebSocketModels;

public class WsEntityList
{
    public List<WsServer> Servers { get; set; }
    public List<WsNetwork> Networks { get; set; }
}