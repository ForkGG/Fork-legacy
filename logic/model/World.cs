using System.IO;
using fork.ViewModel;

namespace fork.Logic.Model
{
    public class World
    {
        private bool isActive;
        private ServerViewModel viewModel;
        
        public string Name { get; set; }
        public DirectoryInfo Directory { get; set; }

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                if (value)
                {
                    viewModel.UpdateActiveWorld(this);
                }
            }
        }

        public World(string name, ServerViewModel viewModel, DirectoryInfo directory)
        {
            Name = name;
            this.viewModel = viewModel;
            Directory = directory;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}