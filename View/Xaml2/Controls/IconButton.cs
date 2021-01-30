using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fork.View.Xaml2.Controls
{
    internal class IconButton : Button
    {
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
            "IconSource", typeof(ImageSource), typeof(IconButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
            "HoverIconSource", typeof(ImageSource), typeof(IconButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            "IconWidth", typeof(double), typeof(IconButton),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            "IconHeight", typeof(double), typeof(IconButton),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            "HoverBackground", typeof(Brush), typeof(IconButton),
            new PropertyMetadata(null));

        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton),
                new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        public ImageSource IconSource
        {
            get => (ImageSource) GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public ImageSource HoverIconSource
        {
            get => (ImageSource) GetValue(HoverIconSourceProperty);
            set => SetValue(HoverIconSourceProperty, value);
        }

        public double IconWidth
        {
            get => (double) GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public double IconHeight
        {
            get => (double) GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
        }

        public Brush HoverBackground
        {
            get => (Brush) GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }
    }
}