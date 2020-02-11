using System.Globalization;
using System.Linq;
using nihilus.Logic.Model;

namespace nihilus.Logic
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
    }
}