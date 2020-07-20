using System;

namespace fork.Logic.Model.APIModel
{
    public class ForkVersion
    {
        internal string Id { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public DateTime ReleaseDay { get; set; }
        public string URL { get; set; }
    }
}