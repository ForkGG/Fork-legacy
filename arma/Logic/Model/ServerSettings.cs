using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nihilus.Logic.Model
{
    public class ServerSettings
    {
        #region Properties
        
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        public string AdminPassword
        {
            get => Settings["passwordAdmin"];
            set => Settings["passwordAdmin"] = value;
        }

        public string Password
        {
            get => Settings["password"];
            set => Settings["password"] = value;
        }

        public string Hostname
        {
            get => Settings["hostname"];
            set => Settings["hostname"] = value;
        }

        public int MaxPlayers
        {
            get => int.Parse(Settings["maxPlayers"]);
            set => Settings["maxPlayers"] = value.ToString();
        }

        public string[] Motd
        {
            get => Settings["motd"].Replace("\"","").Split(',');
            set
            {
                string conc = "";
                foreach (string line in value)
                {
                    conc = conc + "\"" + line + "\",";
                }
                conc = conc.Remove(conc.Length - 1);
                Settings["motd"] = conc;
            }
        }

        public long[] admins
        {
            get
            {
                string[] adminStrings = Settings["admins"].Replace("\"", "")
                    .Replace("{", "")
                    .Replace("}", "")
                    .Split(',');
                List<long> admins = new List<long>();
                foreach (string adminString in adminStrings)
                {
                    admins.Add(long.Parse(adminString));
                }

                return admins.ToArray();
            }

            set
            {
                string conc = "{";
                foreach (long line in value)
                {
                    conc = conc + "\"" + line + "\",";
                }
                conc = conc.Remove(conc.Length - 1);
                Settings["admins"] = conc + "}";
            }
        }
        //TODO add all from https://community.bistudio.com/wiki/server.cfg

        public ServerSettings(Dictionary<string, string> settings)
        {
            Settings = settings;
        }
        
        public  ServerSettings(string name)

        #endregion
    }
}