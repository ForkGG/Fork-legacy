using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fork {

    public class ExtendedRadioButton : RadioButton {

        public ImageSource IconSource { get; set; }

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
          "IconSource", typeof(ImageSource), typeof(ExtendedRadioButton), new PropertyMetadata(null));

        public string Description { get; set; }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
          "Description", typeof(string), typeof(ExtendedRadioButton), new PropertyMetadata(""));

        static ExtendedRadioButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedRadioButton), new FrameworkPropertyMetadata(typeof(ExtendedRadioButton)));
        }

    }

}
