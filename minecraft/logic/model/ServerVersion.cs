using System;

namespace nihilus.logic.model
{
    [Serializable]
    public class ServerVersion
    {
        public string Version { get; set; }
        public string JarLink { get; set; }

        public ServerVersion(){}
        
        public override string ToString()
        {
            return Version;
        }
    }
}