using Avalonia;
using Avalonia.Controls;
using System.Numerics;
using System.Windows;

namespace Partlyx.UI.Avalonia.OtherControls
{
    public class FromToLineControl : Control
    {
        public static readonly AttachedProperty<Vector2> FromProperty =
            AvaloniaProperty.Register(nameof(From), typeof(Vector2), typeof(FromToLineControl),
            new PropertyMetadata(Vector2.Zero, VisualPropertyChanged));
        public Vector2 From { get => (Vector2)GetValue(FromProperty); set => SetValue(FromProperty, value); }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register(nameof(To), typeof(Vector2), typeof(FromToLineControl),
            new PropertyMetadata(Vector2.Zero, VisualPropertyChanged));
        public Vector2 To { get => (Vector2)GetValue(ToProperty); set => SetValue(ToProperty, value); }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register(nameof(Brush), typeof(Brush), typeof(FromToLineControl),
            new PropertyMetadata(null, VisualPropertyChanged));
        public Brush Brush { get => (Brush)GetValue(BrushProperty); set => SetValue(BrushProperty, value); }

        public static readonly DependencyProperty LineThicknessProperty =
            DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(FromToLineControl),
            new PropertyMetadata(2.0, VisualPropertyChanged));
        public double LineThickness { get => (double)GetValue(LineThicknessProperty); set => SetValue(LineThicknessProperty, value); }

        static void VisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ftlc = (FromToLineControl)d;
            ftlc.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            Point p1 = new Point(From.X, From.Y);
            Point p2 = new Point(To.X, To.Y);

            System.Windows.Vector dir = p2 - p1;
            System.Windows.Vector normal = new System.Windows.Vector(-dir.Y, dir.X);
            normal.Normalize();

            var geom = new PathGeometry(new[] {
            new PathFigure(p1, new PathSegment[] { new LineSegment(p2, true) }, false) });

            dc.DrawGeometry(null, new Pen(Brush, LineThickness), geom);
        }
    }

}
