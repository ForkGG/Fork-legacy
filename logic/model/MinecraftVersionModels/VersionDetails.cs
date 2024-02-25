namespace Fork.Logic.Model.MinecraftVersionModels;

public class VersionDetails
{
    public Downloads downloads { get; set; }

    public class Client
    {
        public string sha1 { get; set; }
        public int size { get; set; }
        public string url { get; set; }
    }

    public class Server
    {
        public string sha1 { get; set; }
        public int size { get; set; }
        public string url { get; set; }
    }

    public class Downloads
    {
        public Client client { get; set; }
        public Server server { get; set; }
    }
}