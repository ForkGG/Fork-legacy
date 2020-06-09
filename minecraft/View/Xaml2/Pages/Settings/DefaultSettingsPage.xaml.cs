using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
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
using System.Xml;
using fork;
using fork.Logic.Model.Settings;
using fork.ViewModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Path = System.IO.Path;

namespace Fork.View.Xaml2.Pages.Settings
{
    public partial class DefaultSettingsPage : Page, ISettingsPage
    {
        public SettingsFile SettingsFile { get; set; }
        public string FileName => Path.GetFileNameWithoutExtension(SettingsFile.FileInfo.FullName);
        public string FileExtension => Path.GetExtension(SettingsFile.FileInfo.FullName);

        public DefaultSettingsPage(SettingsFile settingsFile)
        {
            InitializeComponent();
            SettingsFile = settingsFile;
            text.Text = settingsFile.Text;
            text.TextArea.TextView.LineSpacing = 1.45d;
            
            using (Stream s = new MemoryStream(Properties.Resources.YAML)) {
                using (XmlTextReader reader = new XmlTextReader(s)) {
                    text.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            text.Options.IndentationSize = 2;
            //text.Options.ShowSpaces = true;
            //text.Options.ShowColumnRuler = true;
            //text.Options.ColumnRulerPosition = 1;
            text.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.Green);
        }

        public void ReadText()
        {
            SettingsFile.ReadText();
            text.Text = SettingsFile.Text;
        }

        public void SaveSettings()
        {
            string newText = "";
            Application.Current.Dispatcher?.Invoke(() => newText = text.Text);
            if (!SettingsFile.Text.Equals(newText))
            {
                SettingsFile.Text = newText;
                SettingsFile.SaveText();
            }
        }
    }
}
