using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Fork.Logic.Model;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.RoleManagement;

namespace Fork.Logic
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

        public static string FriendlyName(this PluginEnums.Sorting sorting)
        {
            switch (sorting)
            {
                case PluginEnums.Sorting.RATING: return "Most Ratings";
                case PluginEnums.Sorting.DOWNLOADS: return "Downloads";
                case PluginEnums.Sorting.LAST_UPDATE: return "Last Updated";
                case PluginEnums.Sorting.SUBMISSION_DATE: return "Release Date";
                default: throw new ArgumentException("Undefined enum entry in PluginEnums.Sorting.FriendlyName()");
            }
        }

        public static string APIName(this PluginEnums.Sorting sorting)
        {
            switch (sorting)
            {
                case PluginEnums.Sorting.RATING: return "-rating";
                case PluginEnums.Sorting.DOWNLOADS: return "-downloads";
                case PluginEnums.Sorting.LAST_UPDATE: return "-updateDate";
                case PluginEnums.Sorting.SUBMISSION_DATE: return "-releaseDate";
                default: throw new ArgumentException("Undefined enum entry in PluginEnums.Sorting.APIName()");
            }
        }
    }
}