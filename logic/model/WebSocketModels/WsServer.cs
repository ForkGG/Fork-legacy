using Fork.Logic.Model.WebSocketModels.Settings;

namespace Fork.Logic.Model.WebSocketModels
{
    public class WsServer : WsEntity
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public WsJavaSettings JavaSettings { get; set; }
        public WsJarVersion JarVersion { get; set; }
        public bool StartWithFork { get; set; }
        
        public WsRestart Restart1 { get; set; }
        public WsRestart Restart2 { get; set; }
        public WsRestart Restart3 { get; set; }
        public WsRestart Restart4 { get; set; }
        public WsVanillaSettings VanillaSettings { get; set; }
    }
}