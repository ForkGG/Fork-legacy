using System.IO;
using Fork.ViewModel;

namespace Fork.Logic.Model;

public class World
{
    private bool isActive;
    private readonly ServerViewModel viewModel;

    public World(string name, ServerViewModel viewModel, DirectoryInfo directory)
    {
        Name = name;
        this.viewModel = viewModel;
        Directory = directory;
    }

    public string Name { get; set; }
    public DirectoryInfo Directory { get; set; }

    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            if (value)
            {
                viewModel.UpdateActiveWorld(this);
            }
        }
    }


    public override string ToString()
    {
        return Name;
    }
}