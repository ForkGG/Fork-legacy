using Fork.ViewModel;

namespace Fork.Logic.Model.EventArgs
{
    public class PlayerEventArgs : System.EventArgs
    {
        public enum PlayerEventType
        {
            Join, Leave
        }
        
        public PlayerEventType EventType { get; }
        public string PlayerName { get; }
        public ServerViewModel Server { get; }

        public PlayerEventArgs(PlayerEventType eventType, string playerName, ServerViewModel server)
        {
            EventType = eventType;
            PlayerName = playerName;
            Server = server;
        }
    }
}