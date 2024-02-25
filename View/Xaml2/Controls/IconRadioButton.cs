using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fork.View.Xaml2.Controls;

internal class IconRadioButton : RadioButton
{
    public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
        "IconSource", typeof(ImageSource), typeof(IconRadioButton), new PropertyMetadata(null));

    public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
        "HoverIconSource", typeof(ImageSource), typeof(IconRadioButton), new PropertyMetadata(null));

    static IconRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IconRadioButton),
            new FrameworkPropertyMetadata(typeof(IconRadioButton)));
    }

    public ImageSource IconSource { get; set; }

    public ImageSource HoverIconSource { get; set; }
}