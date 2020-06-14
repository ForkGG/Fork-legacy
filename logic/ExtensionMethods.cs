using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using fork.Logic.Model;
using fork.Logic.RoleManagement;

namespace fork.Logic
{
    public static class ExtensionMethods
    {
        public static string FriendlyName(this ServerStatus serverStatus)
        {
            string s = serverStatus.ToString();
            return s.First() + s.Substring(1).ToLower(CultureInfo.InvariantCulture);
            switch (serverStatus)
            {
                case ServerStatus.RUNNING: return "Running";
                case ServerStatus.STARTING: return "Starting";
                case ServerStatus.STOPPED: return "Stopped";
                default: return "Horrible Failure!!!";
            }
        }

        public static string JsonName(this RoleType roleType)
        {
            switch (roleType)
            {
                case RoleType.WHITELIST: return "whitelist.json";
                case RoleType.BAN_LIST: return "banned-players.json";
                case RoleType.OP_LIST: return "ops.json";
                default: throw new ArgumentException("Undefined enum entry in RoleType.JsonName()");
            }
        }

        public static string TxtName(this RoleType roleType)
        {
            switch (roleType)
            {
                case RoleType.WHITELIST: return "white-list.txt";
                case RoleType.OP_LIST: return "ops.txt"; 
                case RoleType.BAN_LIST: return "banned-players.txt"; 
                default: throw new ArgumentException("Undefined enum entry in RoleType.TxtName()");
            }
        }
    }
}