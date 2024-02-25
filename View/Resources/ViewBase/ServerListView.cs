using System.Windows;
using System.Windows.Controls;

namespace Fork.View.Resources.ViewBase;

public class ServerListView : System.Windows.Controls.ViewBase
{
    public static readonly DependencyProperty
        ItemContainerStyleProperty =
            ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(ServerListView));

    public static readonly DependencyProperty ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner(typeof(ServerListView));

    public static readonly DependencyProperty ItemWidthProperty =
        WrapPanel.ItemWidthProperty.AddOwner(typeof(ServerListView));

    public static readonly DependencyProperty ItemHeightProperty =
        WrapPanel.ItemHeightProperty.AddOwner(typeof(ServerListView));

    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    protected override object DefaultStyleKey => new ComponentResourceKey(GetType(), "myServerListViewDSK");
}