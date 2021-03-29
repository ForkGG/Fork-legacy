using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Timers;
using Fork.Annotations;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.WebRequesters;
using Timer = System.Timers.Timer;

namespace Fork.logic.model.PluginModels
{
    public class InstalledPlugin : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public bool IsSpigetPlugin { get; set; }
        public int SpigetId { get; set; }
        public long InstalledVersion { get; set; }
        public bool IsDownloaded { get; set; } = false;
        public bool IsEnabled { get; set; } = true;

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public Plugin Plugin { get; private set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public long LatestVersion { get; private set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public bool Initialized { get; private set; } = false;


        #region events

        public delegate void HandleInitializedEvent();

        public delegate void HandleUpdatedEvent();

        public event HandleInitializedEvent PluginInitializedEvent;
        public event HandleUpdatedEvent PluginUpdateEvent;

        #endregion

        [OnDeserialized]
        public void AfterInit(StreamingContext context)
        {
            if (IsSpigetPlugin)
            {
                PluginWebRequester webRequester = new PluginWebRequester();
                if (Plugin == null)
                {
                    Plugin = webRequester.RequestPlugin(SpigetId);
                }

                LatestVersion = InstalledVersion;
                Timer t = new Timer(1000 * 60 * 60 * 2);
                t.Elapsed += UpdateTwoHour;
                t.AutoReset = true;
                t.Enabled = true;
                Initialized = true;
                PluginInitializedEvent?.Invoke();
            }
        }

        private void UpdateTwoHour(object sender, ElapsedEventArgs e)
        {
            PluginWebRequester webRequester = new PluginWebRequester();
            long newVersion = webRequester.RequestLatestVersion(SpigetId);
            if (newVersion > LatestVersion)
            {
                LatestVersion = newVersion;
                PluginUpdateEvent?.Invoke();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}