using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fork.Logic.Model.ProxyModels
{
    public class Group
    {
        public string User { get; set; }
        public ObservableCollection<string> Groups { get; set; }

        public Group(string user, ObservableCollection<string> groups)
        {
            User = user;
            Groups = groups;
        }

        protected bool Equals(Group other)
        {
            return User == other.User && Equals(Groups, other.Groups);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Group) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((User != null ? User.GetHashCode() : 0) * 397) ^ (Groups != null ? Groups.GetHashCode() : 0);
            }
        }
    }
}