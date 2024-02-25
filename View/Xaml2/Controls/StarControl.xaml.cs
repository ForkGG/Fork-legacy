using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fork.View.Xaml2.Controls;

public partial class StarControl : UserControl
{
    #region Data

    private const int STAR_SIZE = 20;

    #endregion

    #region Ctor

    public StarControl()
    {
        DataContext = this;
        InitializeComponent();

        gdStar.Width = STAR_SIZE;
        gdStar.Height = STAR_SIZE;
        gdStar.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, STAR_SIZE, STAR_SIZE)
        };

        mask.Width = STAR_SIZE;
        mask.Height = STAR_SIZE;
    }

    #endregion


    #region BackgroundColor

    /// <summary>
    ///     BackgroundColor Dependency Property
    /// </summary>
    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register("BackgroundColor",
            typeof(SolidColorBrush), typeof(StarControl),
            new FrameworkPropertyMetadata(Brushes.Transparent,
                OnBackgroundColorChanged));

    /// <summary>
    ///     Gets or sets the BackgroundColor property.
    /// </summary>
    public SolidColorBrush BackgroundColor
    {
        get => (SolidColorBrush)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    ///     Handles changes to the BackgroundColor property.
    /// </summary>
    private static void OnBackgroundColorChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        StarControl control = (StarControl)d;
        control.gdStar.Background = (SolidColorBrush)e.NewValue;
        control.mask.Fill = (SolidColorBrush)e.NewValue;
    }

    #endregion

    #region StarForegroundColor

    /// <summary>
    ///     StarForegroundColor Dependency Property
    /// </summary>
    public static readonly DependencyProperty StarForegroundColorProperty =
        DependencyProperty.Register("StarForegroundColor", typeof(SolidColorBrush),
            typeof(StarControl),
            new FrameworkPropertyMetadata(Brushes.Transparent,
                OnStarForegroundColorChanged));

    /// <summary>
    ///     Gets or sets the StarForegroundColor property.
    /// </summary>
    public SolidColorBrush StarForegroundColor
    {
        get => (SolidColorBrush)GetValue(StarForegroundColorProperty);
        set => SetValue(StarForegroundColorProperty, value);
    }

    /// <summary>
    ///     Handles changes to the StarForegroundColor property.
    /// </summary>
    private static void OnStarForegroundColorChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        StarControl control = (StarControl)d;
        control.starForeground.Fill = (SolidColorBrush)e.NewValue;
    }

    #endregion

    #region StarOutlineColor

    /// <summary>
    ///     StarOutlineColor Dependency Property
    /// </summary>
    public static readonly DependencyProperty StarOutlineColorProperty =
        DependencyProperty.Register("StarOutlineColor", typeof(SolidColorBrush),
            typeof(StarControl),
            new FrameworkPropertyMetadata(Brushes.Transparent,
                OnStarOutlineColorChanged));

    /// <summary>
    ///     Gets or sets the StarOutlineColor property.
    /// </summary>
    public SolidColorBrush StarOutlineColor
    {
        get => (SolidColorBrush)GetValue(StarOutlineColorProperty);
        set => SetValue(StarOutlineColorProperty, value);
    }

    /// <summary>
    ///     Handles changes to the StarOutlineColor property.
    /// </summary>
    private static void OnStarOutlineColorChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        StarControl control = (StarControl)d;
        control.starOutline.Stroke = (SolidColorBrush)e.NewValue;
    }

    #endregion


    #region Value

    /// <summary>
    ///     Value Dependency Property
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(decimal),
            typeof(StarControl),
            new FrameworkPropertyMetadata((decimal)0.0,
                OnValueChanged,
                CoerceValueValue));

    /// <summary>
    ///     Gets or sets the Value property.
    /// </summary>
    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///     Handles changes to the Value property.
    /// </summary>
    private static void OnValueChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(MinimumProperty);
        d.CoerceValue(MaximumProperty);
        StarControl starControl = (StarControl)d;
        if (starControl.Value == 0.0m)
        {
            starControl.starForeground.Fill = Brushes.Gray;
        }
        else
        {
            starControl.starForeground.Fill = starControl.StarForegroundColor;
        }

        int marginLeftOffset = (int)(starControl.Value * STAR_SIZE);
        starControl.mask.Margin = new Thickness(marginLeftOffset, 0, 0, 0);
        starControl.InvalidateArrange();
        starControl.InvalidateMeasure();
        starControl.InvalidateVisual();
    }

    /// <summary>
    ///     Coerces the Value value.
    /// </summary>
    private static object CoerceValueValue(DependencyObject d, object value)
    {
        StarControl starControl = (StarControl)d;
        decimal current = (decimal)value;
        if (current < starControl.Minimum)
        {
            current = starControl.Minimum;
        }

        if (current > starControl.Maximum)
        {
            current = starControl.Maximum;
        }

        return current;
    }

    #endregion

    #region Maximum

    /// <summary>
    ///     Maximum Dependency Property
    /// </summary>
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(decimal),
            typeof(StarControl),
            new FrameworkPropertyMetadata((decimal)1.0));

    /// <summary>
    ///     Gets or sets the Maximum property.
    /// </summary>
    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion

    #region Minimum

    /// <summary>
    ///     Minimum Dependency Property
    /// </summary>
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(decimal),
            typeof(StarControl),
            new FrameworkPropertyMetadata((decimal)0.0));

    /// <summary>
    ///     Gets or sets the Minimum property.
    /// </summary>
    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    #endregion
}