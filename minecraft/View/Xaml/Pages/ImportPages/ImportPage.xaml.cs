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
using nihilus.View.Xaml.Pages.ImportPages;
using nihilus.ViewModel;

namespace nihilus.View.Xaml.Pages
{
    /// <summary>
    /// Interaktionslogik für ImportServerPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        private ImportViewModel viewModel;

        private ImportServerPage importServerPage;
        private ImportWorldPage importWorldPage;

        public Page SecondPage { get; set; }
        
        public ImportPage(ImportViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            importServerPage = new ImportServerPage(this, viewModel);
            importWorldPage = new ImportWorldPage(this, viewModel);
            SecondPage = importServerPage;
        }

        private void ImportTypeServer_Click(object sender, RoutedEventArgs e)
        {
            SecondPage = importServerPage;
        }

        private void ImportTypeWorld_Click(object sender, RoutedEventArgs e)
        {
            SecondPage = importWorldPage;
        }
        
        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.VanillaServerVersions });
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RaiseImportCloseEvent(sender, e);
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RaiseImportNextEvent(sender, e);
        }
    }
}
