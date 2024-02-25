using System.Collections.ObjectModel;

namespace Fork.Logic.Model.ProxyModels;

public class Permission
{
    public Permission(string name, ObservableCollection<string> permissions)
    {
        Name = name;
        PermissionList = permissions;
    }

    public string Name { get; set; }
    public ObservableCollection<string> PermissionList { get; set; }
}