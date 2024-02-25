using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fork.View.Xaml2.Shapes;

public class SpeechBubble : Shape
{
    protected override Geometry DefiningGeometry => GenerateSpeechBubble();

    private Geometry GenerateSpeechBubble()
    {
        StreamGeometry geom = new();
        using (StreamGeometryContext gc = geom.Open())
        {
            Size cornerSize = new(10, 10);
            Point start = new(20, 0);
            gc.BeginFigure(start, true, true);
            gc.LineTo(new Point(440, 0), true, true);
            gc.ArcTo(new Point(450, 10), cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
            gc.LineTo(new Point(450, 70), true, true);
            gc.ArcTo(new Point(440, 80), cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
            gc.LineTo(new Point(20, 80), true, true);
            gc.ArcTo(new Point(10, 70), cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
            gc.LineTo(new Point(10, 55), true, true);
            gc.LineTo(new Point(-10, 40), true, true);
            gc.LineTo(new Point(10, 25), true, true);

            gc.LineTo(new Point(10, 10), true, true);
            gc.ArcTo(start, cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
        }

        return geom;
    }
}