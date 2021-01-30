namespace Fork.Logic.Model
{
    public interface Entity
    {
        string UID { get; set; }
        bool Initialized { get; set; }
        JavaSettings JavaSettings { get; set; }
        ServerVersion Version { get; set; }
        string Name { get; set; }
        bool StartWithFork { get; set; }
        int ServerIconId { get; set; }

        string ToString();
    }
}