using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Fork.Logic.Model
{
    [Serializable]
    public class ServerVersion : IComparable
    {
        public enum VersionType
        {
            Vanilla,
            Paper,
            Spigot,
            Waterfall,
            BungeeCord,
            Snapshot,
            Purpur,
            Fabric,
        }

        private Regex nonNumeric = new Regex(@"[^\d.]");

        public int Id { get; set; }
        public VersionType Type { get; set; }
        public string Version { get; set; }
        public int Build { get; set; } = 0;
        public string JarLink { get; set; }
        [JsonIgnore] public bool IsProxy => Type == VersionType.Waterfall;
        [JsonIgnore] public bool SupportBuilds => Type == VersionType.Paper;
        [JsonIgnore] public bool HasPlugins => Type == VersionType.Paper || Type == VersionType.Purpur || Type == VersionType.Spigot;

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
                string friendlyVersion = nonNumeric.Replace(this.Version, "");
                List<string> thisVersionSub = new List<string>(friendlyVersion.Split('.'));
                this.Version.Split('.');
                thisVersionSub.Add("0");
                thisVersionSub.Add("0");
                string friendlyOtherVersion = nonNumeric.Replace(otherVersion.Version, "");
                List<string> otherVersionSub = new List<string>(friendlyOtherVersion.Split('.'));
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

        protected bool Equals(ServerVersion other)
        {
            return Type == other.Type && Version == other.Version && JarLink == other.JarLink;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is ServerVersion otherVersion)
            {
                return Equals(otherVersion);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (JarLink != null ? JarLink.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}