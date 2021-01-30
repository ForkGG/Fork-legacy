using System.Windows;
using System.Windows.Forms;
using Fork.ViewModel;
using Button = System.Windows.Controls.Button;

namespace Fork.View.Resources.dictionaries
{
    public partial class ListBoxStyles : ResourceDictionary
    {
        public void RemoveFromNetwork_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            EntityViewModel viewModel = button?.DataContext as EntityViewModel;

            if (viewModel == null) return;

            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = false, Filter = "Server Icon|*.png;*.jpg;*.jpeg;*.bmp;*.tiff",
                Title = "Select your server icon"
            };
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                viewModel.UpdateCustomImage(ofd.FileName);
        }
    }
}