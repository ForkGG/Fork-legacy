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
using System.Windows.Navigation;
using System.Windows.Shapes;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml2.Pages
{
    /// <summary>
    /// Interaktionslogik für ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private ServerViewModel viewModel;
        
        public ServerPage(ServerViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }
    }
}
