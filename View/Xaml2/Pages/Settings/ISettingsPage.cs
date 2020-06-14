using System;
using System.IO;
using System.Windows.Controls;
using fork.Logic.Model.Settings;

namespace Fork.View.Xaml2.Pages.Settings
{
    public interface ISettingsPage
    {
        SettingsFile SettingsFile { get; set; }
        string FileName { get; }
        string FileExtension { get; }
        void SaveSettings();
    }
}