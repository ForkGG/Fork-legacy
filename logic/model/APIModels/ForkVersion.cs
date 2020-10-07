using System;

namespace Fork.Logic.Model.APIModels
{
    public class ForkVersion : IComparable, IComparable<ForkVersion>
    {
        internal string Id { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public DateTime ReleaseDay { get; set; }
        public string URL { get; set; }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is ForkVersion other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ForkVersion)}");
        }

        public int CompareTo(ForkVersion other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;
            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;
            return Patch.CompareTo(other.Patch);
        }

        protected bool Equals(ForkVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ForkVersion) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }
    }
}