using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fork.Logic.Model.ProxyModels
{
    public class Permission
    {
        public string Name { get; set; }
        public ObservableCollection<string> PermissionList { get; set; }

        public Permission(string name, ObservableCollection<string> permissions)
        {
            Name = name;
            PermissionList = permissions;
        }
    }
}