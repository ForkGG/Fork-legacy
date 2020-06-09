using System;
using System.Collections.Generic;
using System.IO;
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
using fork.Logic.Model.Settings;
using fork.ViewModel;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Path = System.IO.Path;

namespace Fork.View.Xaml2.Pages.Settings
{
    public partial class DefaultSettingsPage : Page
    {
        public DefaultSettingsPage(SettingsFile settingsFile)
        {
            InitializeComponent();
            text.Text = settingsFile.Text;
        }
    }
}
