using System;
using System.Collections.Generic;

namespace nihilus.Logic.Model.MinecraftVersionPojo
{
    public class Manifest
    {
        public enum VersionType
        {
            release, snapshot,old_beta,old_alpha
        }
        
        public Latest latest { get; set; }
        public List<Version> versions { get; set; }
        
        public class Latest
        {
            public string release { get; set; }
            public string snapshot { get; set; }
        }

        public class Version
        {
            public string id { get; set; }
            public VersionType type { get; set; }
            public string url { get; set; }
            public DateTime time { get; set; }
            public DateTime releaseTime { get; set; }
        }
    }
}