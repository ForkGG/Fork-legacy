using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Timers;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.WebRequesters;
using Timer = System.Timers.Timer;

namespace Fork.logic.model.PluginModels
{
    public class InstalledPlugin
    {
        public string Name { get; set; }
        public bool IsSpigetPlugin { get; set; }
        public int SpigetId { get; set; }
        public string InstalledVersion { get; set; }
        public bool IsDownloaded { get; set; } = false;

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public Plugin Plugin { get; private set; }
        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public string LatestVersion { get; private set; }
        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public bool Initialized { get; private set; } = false;


        #region events
        public delegate void HandleInitializedEvent();
        public delegate void HandleUpdatedEvent();
        public event HandleInitializedEvent PluginInitializedEvent;
        public event HandleUpdatedEvent PluginUpdateEvent;
        #endregion

        [OnSerialized]
        public void AfterInit()
        {
            if (IsSpigetPlugin)
            {
                PluginWebRequester webRequester = new PluginWebRequester();
                new Thread(() =>
                {
                    Plugin = webRequester.RequestPlugin(SpigetId);
                    LatestVersion = InstalledVersion;
                    Timer t = new Timer(1000*60*60*2);
                    t.Elapsed += UpdateTwoHour;
                    t.AutoReset = true;
                    t.Enabled = true;
                    Initialized = true;
                    PluginInitializedEvent?.Invoke();
                }).Start();
            }
        }

        private void UpdateTwoHour(object sender, ElapsedEventArgs e)
        {
            PluginWebRequester webRequester = new PluginWebRequester();
            string newVersion = webRequester.RequestLatestVersion(SpigetId);
            if (!newVersion.Equals(LatestVersion))
            {
                LatestVersion = newVersion;
                PluginUpdateEvent?.Invoke();
            }
        }
    }
}