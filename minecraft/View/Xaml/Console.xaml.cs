using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
    }
}
