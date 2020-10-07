namespace Fork.Logic.Persistence.PersistencePO
{
    public class BannedPlayer
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string created { get; set; }
        public string source { get; set; }
        public string expires { get; set; }
        public string reason { get; set; }
    }
}