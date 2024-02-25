using System.Windows;
using System.Windows.Media;

namespace Fork;

public partial class WindowStyles : ResourceDictionary
{
    //private Console Console = new Console();

    private void BtnCloseClick(object sender, RoutedEventArgs e)
    {
        Window window = GetParentWindow((DependencyObject)sender);
        window.Close();
    }

    private void BtnMaximizeClick(object sender, RoutedEventArgs e)
    {
        Window window = GetParentWindow((DependencyObject)sender);
        if (window.WindowState == WindowState.Normal)
        {
            window.WindowState = WindowState.Maximized;
        }
        else
        {
            window.WindowState = WindowState.Normal;
        }
    }

    private void BtnMinimizeClick(object sender, RoutedEventArgs e)
    {
        Window window = GetParentWindow((DependencyObject)sender);
        window.WindowState = WindowState.Minimized;
    }

    private void BtnShowConsoleClick(object sender, RoutedEventArgs e)
    {
        //Console.Show();
    }

    public static Window GetParentWindow(DependencyObject child)
    {
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        if (parentObject == null)
        {
            return null;
        }

        Window parent = parentObject as Window;
        if (parent != null)
        {
            return parent;
        }

        return GetParentWindow(parentObject);
    }
}