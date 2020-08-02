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
    class ServerTypeRadioButton : RadioButton
    {
        public ImageSource IconSource { get; set; }
        
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
            "IconSource", typeof(ImageSource), typeof(ServerTypeRadioButton), new PropertyMetadata(null));
        
        public ImageSource HoverIconSource { get; set; }
        
        public static readonly DependencyProperty HoverIconSourceProperty = DependencyProperty.Register(
            "HoverIconSource", typeof(ImageSource), typeof(ServerTypeRadioButton), new PropertyMetadata(null));
        
        public string Header { get; set; }
        
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(ServerTypeRadioButton), new PropertyMetadata(""));

        static ServerTypeRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ServerTypeRadioButton), new FrameworkPropertyMetadata(typeof(ServerTypeRadioButton)));
        }
    }
}
