using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using nihilus.Logic.Manager;
using nihilus.ViewModel;

namespace nihilus.View.Xaml
{
    /// <summary>
    /// Interaktionslogik für Console.xaml
    /// </summary>
    public partial class Console : Window
    {
        private readonly ConsoleViewModel viewModel = ApplicationManager.Instance.ConsoleViewModel;
        
        public Console()
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += Console_Loaded;
            Closing += Window_Closing;
        }
        
        void Console_Loaded(object sender, RoutedEventArgs e)
        {
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }

        void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                viewModel.ConsoleInput = InputBlock.Text;
                viewModel.RunCommand();
                InputBlock.Focus();
                Scroller.ScrollToBottom();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
