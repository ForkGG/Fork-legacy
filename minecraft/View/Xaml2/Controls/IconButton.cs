using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;

namespace nihilus.View.Xaml2.Controls
{
    class IconButton : Button
    {
        public ImageSource IconSource
        {
            get
            {
                return (ImageSource) GetValue(IconSourceProperty);
            }
            set
            {
                SetValue(IconSourceProperty, value);
            }
        }

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
            "IconSource", typeof(ImageSource), typeof(IconButton), 
            new PropertyMetadata(null, OnIconSourceChanged));
        
        public ImageSource HoverIconSource {
            get
            {
                return (ImageSource) GetValue(HoverIconSourceProperty);
            }
            set
            {
                SetValue(HoverIconSourceProperty, value);
            } 
        }
        
        public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
            "HoverIconSource", typeof(ImageSource), typeof(IconButton), 
            new PropertyMetadata(null, OnHoverIconSourceChanged));
        
        public double IconWidth { get
            {
                return (double) GetValue(IconWidthProperty);
            }
            set
            {
                SetValue(IconWidthProperty, value);
            }  }
        
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            "IconWidth", typeof(double), typeof(IconButton), 
            new PropertyMetadata(0.0, OnIconWidthChanged));

        public double IconHeight
        {
            get
            {
                return (double) GetValue(IconHeightProperty);
            }
            set
            {
                SetValue(IconHeightProperty, value);
            } 
        }
        
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            "IconHeight", typeof(double), typeof(IconButton), 
            new PropertyMetadata(0.0, OnIconHeightChanged));

        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (IconButton) d;
            button.IconSource = (ImageSource) e.NewValue;
        }
        private static void OnHoverIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (IconButton) d;
            button.HoverIconSource = (ImageSource) e.NewValue;
        }
        
        private static void OnIconHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (IconButton) d;
            button.IconHeight = (double) e.NewValue;
        }
        private static void OnIconWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (IconButton) d;
            button.IconWidth = (double) e.NewValue;
        }
    }
}
