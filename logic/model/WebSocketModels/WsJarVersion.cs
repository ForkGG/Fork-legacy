namespace Fork.Logic.Model.WebSocketModels;

public class WsJarVersion
{
    public enum JarVersionType
    {
        Vanilla,
        Paper,
        Spigot,
        Waterfall,
        BungeeCord,
        Snapshot,
        Purpur,
        Fabric
    }

    public JarVersionType VersionType { get; set; }
    public string Version { get; set; }
    public int Build { get; set; } = 0;
}