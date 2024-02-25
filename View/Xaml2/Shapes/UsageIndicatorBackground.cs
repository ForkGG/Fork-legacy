using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fork.View.Xaml2.Shapes;

public class UsageIndicatorBackground : Shape
{
    protected override Geometry DefiningGeometry => GenerateUsageIndicatorBackground();

    private Geometry GenerateUsageIndicatorBackground()
    {
        StreamGeometry geom = new();
        using (StreamGeometryContext gc = geom.Open())
        {
            Point start = new(40, 0);
            gc.BeginFigure(start, true, true);
            gc.LineTo(new Point(75, 0), true, true);
            gc.ArcTo(new Point(80, 5), new Size(5, 5), 0.0, false, SweepDirection.Clockwise, true, true);
            gc.LineTo(new Point(80, 50), true, true);
            gc.ArcTo(new Point(75, 55), new Size(5, 5), 0.0, false, SweepDirection.Clockwise, true, true);
            gc.LineTo(new Point(40, 55), true, true);
            gc.ArcTo(start, new Size(13, 18), 0.0, false, SweepDirection.Clockwise, true, true);
        }

        return geom;
    }
}