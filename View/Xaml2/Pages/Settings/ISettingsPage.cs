using System.Threading.Tasks;
using Fork.Logic.Model.Settings;

namespace Fork.View.Xaml2.Pages.Settings;

public interface ISettingsPage
{
    SettingsFile SettingsFile { get; set; }
    string FileName { get; }
    string FileExtension { get; }
    Task SaveSettings();
}