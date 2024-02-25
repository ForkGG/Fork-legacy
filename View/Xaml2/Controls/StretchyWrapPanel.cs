﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Fork.View.Xaml2.Controls;

public class StretchyWrapPanel : Panel
{
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof(ItemWidth),
        typeof(double), typeof(StretchyWrapPanel),
        new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight),
        typeof(double), typeof(StretchyWrapPanel),
        new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(
        typeof(StretchyWrapPanel),
        new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure,
            OnOrientationChanged));

    public static readonly DependencyProperty StretchProportionallyProperty = DependencyProperty.Register(
        nameof(StretchProportionally), typeof(bool),
        typeof(StretchyWrapPanel), new PropertyMetadata(true, OnStretchProportionallyChanged));

    private Orientation _orientation = Orientation.Horizontal;

    private bool _stretchProportionally = true;

    [TypeConverter(typeof(LengthConverter))]
    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    [TypeConverter(typeof(LengthConverter))]
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set => SetValue(OrientationProperty, value);
    }

    public bool StretchProportionally
    {
        get => _stretchProportionally;
        set => SetValue(StretchProportionallyProperty, value);
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((StretchyWrapPanel)d)._orientation = (Orientation)e.NewValue;
    }

    private static void OnStretchProportionallyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        ((StretchyWrapPanel)o)._stretchProportionally = (bool)e.NewValue;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        UVSize curLineSize = new UVSize(Orientation);
        UVSize panelSize = new UVSize(Orientation);
        UVSize uvConstraint = new UVSize(Orientation, constraint.Width, constraint.Height);
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        bool itemWidthSet = !double.IsNaN(itemWidth);
        bool itemHeightSet = !double.IsNaN(itemHeight);

        Size childConstraint = new Size(
            itemWidthSet ? itemWidth : constraint.Width,
            itemHeightSet ? itemHeight : constraint.Height);

        UIElementCollection children = InternalChildren;

        for (int i = 0, count = children.Count; i < count; i++)
        {
            UIElement child = children[i];
            if (child == null)
            {
                continue;
            }

            // Flow passes its own constrint to children
            child.Measure(childConstraint);

            // This is the size of the child in UV space
            UVSize sz = new UVSize(Orientation,
                itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);

            if (curLineSize.U + sz.U > uvConstraint.U)
            {
                // Need to switch to another line
                panelSize.U = Math.Max(curLineSize.U, panelSize.U);
                panelSize.V += curLineSize.V;
                curLineSize = sz;

                if (sz.U > uvConstraint.U)
                {
                    // The element is wider then the constrint - give it a separate line             
                    panelSize.U = Math.Max(sz.U, panelSize.U);
                    panelSize.V += sz.V;
                    curLineSize = new UVSize(Orientation);
                }
            }
            else
            {
                // Continue to accumulate a line
                curLineSize.U += sz.U;
                curLineSize.V = Math.Max(sz.V, curLineSize.V);
            }
        }

        // The last line size, if any should be added
        panelSize.U = Math.Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V;

        // Go from UV space to W/H space
        return new Size(panelSize.Width, panelSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int firstInLine = 0;
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        double accumulatedV = 0;
        double itemU = Orientation == Orientation.Horizontal ? itemWidth : itemHeight;
        UVSize curLineSize = new UVSize(Orientation);
        UVSize uvFinalSize = new UVSize(Orientation, finalSize.Width, finalSize.Height);
        bool itemWidthSet = !double.IsNaN(itemWidth);
        bool itemHeightSet = !double.IsNaN(itemHeight);
        bool useItemU = Orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;

        UIElementCollection children = InternalChildren;

        for (int i = 0, count = children.Count; i < count; i++)
        {
            UIElement child = children[i];
            if (child == null)
            {
                continue;
            }

            UVSize sz = new UVSize(Orientation, itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);
            if (curLineSize.U + sz.U > uvFinalSize.U)
            {
                // Need to switch to another line
                if (!useItemU && StretchProportionally)
                {
                    ArrangeLineProportionally(accumulatedV, curLineSize.V, firstInLine, i, uvFinalSize.Width);
                }
                else
                {
                    ArrangeLine(accumulatedV, curLineSize.V, firstInLine, i, true,
                        useItemU ? itemU : uvFinalSize.Width / Math.Max(1, i - firstInLine - 1));
                }

                accumulatedV += curLineSize.V;
                curLineSize = sz;

                if (sz.U > uvFinalSize.U)
                {
                    // The element is wider then the constraint - give it a separate line     
                    // Switch to next line which only contain one element
                    if (!useItemU && StretchProportionally)
                    {
                        ArrangeLineProportionally(accumulatedV, sz.V, i, ++i, uvFinalSize.Width);
                    }
                    else
                    {
                        ArrangeLine(accumulatedV, sz.V, i, ++i, true, useItemU ? itemU : uvFinalSize.Width);
                    }

                    accumulatedV += sz.V;
                    curLineSize = new UVSize(Orientation);
                }

                firstInLine = i;
            }
            else
            {
                // Continue to accumulate a line
                curLineSize.U += sz.U;
                curLineSize.V = Math.Max(sz.V, curLineSize.V);
            }
        }

        // Arrange the last line, if any
        if (firstInLine < children.Count)
        {
            if (!useItemU && StretchProportionally)
            {
                ArrangeLineProportionally(accumulatedV, curLineSize.V, firstInLine, children.Count,
                    uvFinalSize.Width);
            }
            else
            {
                ArrangeLine(accumulatedV, curLineSize.V, firstInLine, children.Count, true,
                    useItemU ? itemU : uvFinalSize.Width / Math.Max(1, children.Count - firstInLine - 1));
            }
        }

        return finalSize;
    }

    private void ArrangeLineProportionally(double v, double lineV, int start, int end, double limitU)
    {
        double u = 0d;
        bool horizontal = Orientation == Orientation.Horizontal;
        UIElementCollection children = InternalChildren;

        double total = 0d;
        for (int i = start; i < end; i++)
        {
            total += horizontal ? children[i].DesiredSize.Width : children[i].DesiredSize.Height;
        }

        double uMultipler = limitU / total;
        for (int i = start; i < end; i++)
        {
            UIElement child = children[i];
            if (child != null)
            {
                UVSize childSize = new UVSize(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                double layoutSlotU = childSize.U * uMultipler;
                child.Arrange(new Rect(horizontal ? u : v, horizontal ? v : u,
                    horizontal ? layoutSlotU : lineV, horizontal ? lineV : layoutSlotU));
                u += layoutSlotU;
            }
        }
    }

    private void ArrangeLine(double v, double lineV, int start, int end, bool useItemU, double itemU)
    {
        double u = 0d;
        bool horizontal = Orientation == Orientation.Horizontal;
        UIElementCollection children = InternalChildren;
        for (int i = start; i < end; i++)
        {
            UIElement child = children[i];
            if (child != null)
            {
                UVSize childSize = new UVSize(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                double layoutSlotU = useItemU ? itemU : childSize.U;
                child.Arrange(new Rect(horizontal ? u : v, horizontal ? v : u,
                    horizontal ? layoutSlotU : lineV, horizontal ? lineV : layoutSlotU));
                u += layoutSlotU;
            }
        }
    }

    private struct UVSize
    {
        internal UVSize(Orientation orientation, double width, double height)
        {
            U = V = 0d;
            _orientation = orientation;
            Width = width;
            Height = height;
        }

        internal UVSize(Orientation orientation)
        {
            U = V = 0d;
            _orientation = orientation;
        }

        internal double U;
        internal double V;
        private readonly Orientation _orientation;

        internal double Width
        {
            get => _orientation == Orientation.Horizontal ? U : V;
            set
            {
                if (_orientation == Orientation.Horizontal)
                {
                    U = value;
                }
                else
                {
                    V = value;
                }
            }
        }

        internal double Height
        {
            get => _orientation == Orientation.Horizontal ? V : U;
            set
            {
                if (_orientation == Orientation.Horizontal)
                {
                    V = value;
                }
                else
                {
                    U = value;
                }
            }
        }
    }
}