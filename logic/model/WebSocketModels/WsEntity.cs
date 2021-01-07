using Fork.Logic.Model.WebSocketModels.Settings;

namespace Fork.Logic.Model.WebSocketModels
{
    public interface WsEntity
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public WsJavaSettings JavaSettings { get; set; }
        public WsJarVersion JarVersion { get; set; }
        public bool StartWithFork { get; set; }
    }
}