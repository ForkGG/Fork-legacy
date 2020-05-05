using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace nihilus.View.Xaml2.Controls
{
    class IconButton : Button
    {
        public ImageSource IconSource { get; set; }
        
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
            "IconSource", typeof(ImageSource), typeof(IconButton), new PropertyMetadata(null));
        
        public ImageSource HoverIconSource { get; set; }
        
        public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
            "HoverIconSource", typeof(ImageSource), typeof(IconButton), new PropertyMetadata(null));
        
        public double IconWidth { get; set; }
        
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            "IconWidth", typeof(double), typeof(IconButton), new PropertyMetadata(0.0));
        
        public double IconHeight { get; set; }
        
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            "IconHeight", typeof(double), typeof(IconButton), new PropertyMetadata(0.0));

        static IconButton()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }
    }
}
