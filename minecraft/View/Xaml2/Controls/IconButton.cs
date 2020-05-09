using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;

namespace fork.View.Xaml2.Controls
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
            new PropertyMetadata(null));
        
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
            new PropertyMetadata(null));
        
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
            new PropertyMetadata(0.0));

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
            new PropertyMetadata(0.0));
        
        public Brush HoverBackground
        {
            get
            {
                return (Brush) GetValue(HoverBackgroundProperty);
            }
            set
            {
                SetValue(HoverBackgroundProperty, value);
            } 
        }
        
        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            "HoverBackground", typeof(Brush), typeof(IconButton), 
            new PropertyMetadata(null));

        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }
    }
}
