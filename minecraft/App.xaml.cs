using System.Windows;
using nihilus.Logic.Manager;

namespace nihilus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ExitApplication(object sender, ExitEventArgs exitEventArgs)
        {
            ApplicationManager.Instance.ExitApplication();
        }
    }
}
