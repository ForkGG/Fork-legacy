using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace fork.View.Resources.ViewBase
{
    public class ServerListView : System.Windows.Controls.ViewBase
    {
        public static readonly DependencyProperty 
            ItemContainerStyleProperty =
                ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(ServerListView));
        
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            ItemsControl.ItemTemplateProperty.AddOwner(typeof(ServerListView));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemWidthProperty =
            WrapPanel.ItemWidthProperty.AddOwner(typeof(ServerListView));

        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }           
            set { SetValue(ItemWidthProperty, value); }
        }

        public static readonly DependencyProperty ItemHeightProperty =
            WrapPanel.ItemHeightProperty.AddOwner(typeof(ServerListView));

        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        protected override object DefaultStyleKey
        {
            get 
            { 
                return new ComponentResourceKey(GetType(), "myServerListViewDSK"); 
            }
        }
    }
}
