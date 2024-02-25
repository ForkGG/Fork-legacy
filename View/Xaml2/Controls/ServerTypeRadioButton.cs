using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fork.View.Xaml2.Controls;

internal class ServerTypeRadioButton : RadioButton
{
    public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
        "IconSource", typeof(ImageSource), typeof(ServerTypeRadioButton), new PropertyMetadata(null));

    public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
        "HoverIconSource", typeof(ImageSource), typeof(ServerTypeRadioButton), new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        "Header", typeof(string), typeof(ServerTypeRadioButton), new PropertyMetadata(""));

    static ServerTypeRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ServerTypeRadioButton),
            new FrameworkPropertyMetadata(typeof(ServerTypeRadioButton)));
    }

    public ImageSource IconSource { get; set; }

    public ImageSource HoverIconSource { get; set; }

    public string Header { get; set; }
}