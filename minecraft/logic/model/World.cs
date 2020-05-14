using fork.ViewModel;

namespace fork.Logic.Model
{
    public class World
    {
        private bool isActive;
        private ServerViewModel viewModel;
        
        public string Name { get; set; }

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

        public World(string name, ServerViewModel viewModel)
        {
            Name = name;
            this.viewModel = viewModel;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}