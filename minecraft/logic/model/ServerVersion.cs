using System;
using System.Collections.Generic;

namespace nihilus.Logic.Model
{
    [Serializable]
    public class ServerVersion : IComparable
    {
        public enum VersionType
        {
            Vanilla,
            Paper,
            Spigot
        }

        public VersionType Type { get; set; }
        public string Version { get; set; }
        public string JarLink { get; set; }

        public ServerVersion()
        {
        }

        public override string ToString()
        {
            return Version;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            ServerVersion otherVersion = obj as ServerVersion;
            if (otherVersion != null)
            {
                List<string> thisVersionSub = new List<string>(this.Version.Split('.'));
                this.Version.Split('.');
                thisVersionSub.Add("0");
                thisVersionSub.Add("0");
                List<string> otherVersionSub = new List<string>(otherVersion.Version.Split('.'));
                otherVersion.Version.Split('.');
                otherVersionSub.Add("0");
                otherVersionSub.Add("0");

                if (int.Parse(thisVersionSub[0]) < int.Parse(otherVersionSub[0]))
                {
                    return -1;
                }

                if (int.Parse(thisVersionSub[0]) > int.Parse(otherVersionSub[0]))
                {
                    return 1;
                }

                if (int.Parse(thisVersionSub[1]) < int.Parse(otherVersionSub[1]))
                {
                    return -1;
                }

                if (int.Parse(thisVersionSub[1]) > int.Parse(otherVersionSub[1]))
                {
                    return 1;
                }

                if (int.Parse(thisVersionSub[2]) < int.Parse(otherVersionSub[2]))
                {
                    return -1;
                }

                if (int.Parse(thisVersionSub[2]) > int.Parse(otherVersionSub[2]))
                {
                    return 1;
                }
                return 0;
            }

            throw new ArgumentException("Object is not a Minecraft Version");
        }
    }
}