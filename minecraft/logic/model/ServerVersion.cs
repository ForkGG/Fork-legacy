using System;

namespace nihilus.Logic.Model
{
    [Serializable]
    public class ServerVersion
    {
        public enum VersionType
        {
            Vanilla, Spigot
        }

        public VersionType Type { get; set; }
        public string Version { get; set; }
        public string JarLink { get; set; }

        public ServerVersion(){}
        
        public override string ToString()
        {
            return Version;
        }
    }
}