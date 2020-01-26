using System;

namespace nihilus.Logic.Model
{
    [Serializable]
    public class ServerRestart
    {
        public bool Enabled { get; set; }
        public SimpleTime Time { get; set; }

        public ServerRestart(bool enabled, SimpleTime time)
        {
            Enabled = enabled;
            Time = time;
        }
        
        public ServerRestart(){}

        public ServerRestart(ServerRestart restart)
        {
            Enabled = restart.Enabled;
            Time = new SimpleTime(restart.Time.Hours,restart.Time.Minutes);
        }
    }
}