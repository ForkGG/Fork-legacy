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
using Fork;
using Fork.Logic;
using Fork.Logic.Model.Settings;
using Fork.Logic.RoleManagement;
using Fork.View.Resources.Folding;
using Fork.ViewModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Path = System.IO.Path;

namespace Fork.View.Xaml2.Pages.Settings
{
    public partial class DefaultSettingsPage : Page, ISettingsPage
    {
        private FoldingManager foldingManager;
        private TabFoldingStrategy tabFoldingStrategy;
        
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
            text.TextArea.TextView.LinkTextForegroundBrush =
                (SolidColorBrush) new BrushConverter().ConvertFrom("#79CF93");
            
            foldingManager = FoldingManager.Install(text.TextArea);
            tabFoldingStrategy = new TabFoldingStrategy();
            UpdateFoldings(foldingManager, tabFoldingStrategy);
            
            text.TextChanged += TextChanged;
            settingsFile.TextReadUpdateEvent += HandleTextReadUpdate;
        }

        public void SaveSettings()
        {
            string newText = "";
            if (SettingsFile.Text == null)
            {
                Console.WriteLine("Failed to save file "+SettingsFile.FileInfo.FullName+" because the text has not yet been read from the file.");
                return;
            }
            Application.Current.Dispatcher?.Invoke(() => newText = text.Text);
            if (!SettingsFile.Text.Equals(newText))
            {
                SettingsFile.Text = newText;
                SettingsFile.SaveText();
            }
        }

        private void HandleTextReadUpdate(object sender, SettingsFile.TextReadUpdatedEventArgs eventArgs)
        {
            Application.Current.Dispatcher?.Invoke(() => text.Text = eventArgs.NewText);
        }

        private void UpdateFoldings(FoldingManager foldingManager, TabFoldingStrategy tabFoldingStrategy)
        {
            tabFoldingStrategy.UpdateFoldings(foldingManager, text.TextArea.Document);
        }

        private void TextChanged(object sender, EventArgs e)
        {
            UpdateFoldings(foldingManager, tabFoldingStrategy);
        }
    }
}
