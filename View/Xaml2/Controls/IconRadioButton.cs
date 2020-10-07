using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fork.View.Xaml2.Controls
{
    class IconRadioButton : RadioButton
    {
        public ImageSource IconSource { get; set; }
        
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
            "IconSource", typeof(ImageSource), typeof(IconRadioButton), new PropertyMetadata(null));
        
        public ImageSource HoverIconSource { get; set; }
        
        public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
            "HoverIconSource", typeof(ImageSource), typeof(IconRadioButton), new PropertyMetadata(null));

        static IconRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconRadioButton), new FrameworkPropertyMetadata(typeof(IconRadioButton)));
        }
    }
}
