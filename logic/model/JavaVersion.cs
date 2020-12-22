namespace Fork.Logic.Model
{
    public class JavaVersion
    {
        public string Version { get; set; }
        public int VersionComputed { get; set; }
        public bool Is64Bit { get; set; } = false;
    }
}