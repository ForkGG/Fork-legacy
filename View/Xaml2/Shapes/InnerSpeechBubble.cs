using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fork.View.Xaml2.Shapes
{
    public class InnerSpeechBubble : Shape
    {
        protected override Geometry DefiningGeometry => GenerateInnerSpeechBubble();

        private Geometry GenerateInnerSpeechBubble()
        {
            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext gc = geom.Open())
            {
                Size cornerSize = new Size(5, 5);
                Point start = new Point(20, 15);
                gc.BeginFigure(start, true, true);
                gc.LineTo(new Point(415, 15), true, true);
                gc.ArcTo(new Point(420, 20), cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
                gc.LineTo(new Point(420, 60), true, true);
                gc.ArcTo(new Point(415, 65), cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
                gc.LineTo(new Point(20, 65), true, true);
                gc.ArcTo(new Point(15, 60), cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
                gc.LineTo(new Point(15, 48), true, true);
                gc.LineTo(new Point(2, 40), true, true);
                gc.LineTo(new Point(15, 32), true, true);

                gc.LineTo(new Point(15, 20), true, true);
                gc.ArcTo(start, cornerSize, 0.0, false, SweepDirection.Clockwise, true, true);
            }

            return geom;
        }
    }
}