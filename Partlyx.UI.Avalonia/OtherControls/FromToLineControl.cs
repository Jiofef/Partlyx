using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Numerics;
using Material.Avalonia;

namespace Partlyx.UI.Avalonia.OtherControls
{
    public class FromToLineControl : Control
    {
        public static readonly StyledProperty<Vector2> FromProperty =
            AvaloniaProperty.Register<FromToLineControl, Vector2>(
                nameof(From),
                Vector2.Zero);

        public static readonly StyledProperty<Vector2> ToProperty =
            AvaloniaProperty.Register<FromToLineControl, Vector2>(
                nameof(To),
                Vector2.Zero);

        public static readonly StyledProperty<IBrush?> BrushProperty =
            AvaloniaProperty.Register<FromToLineControl, IBrush?>(
                nameof(Brush),
                null);

        public static readonly StyledProperty<double> LineThicknessProperty =
            AvaloniaProperty.Register<FromToLineControl, double>(
                nameof(LineThickness),
                2.0);

        public Vector2 From
        {
            get => GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public Vector2 To
        {
            get => GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public IBrush? Brush
        {
            get => GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public double LineThickness
        {
            get => GetValue(LineThicknessProperty);
            set => SetValue(LineThicknessProperty, value);
        }

        static FromToLineControl()
        {
            AffectsRender<FromToLineControl>(
                FromProperty, ToProperty, BrushProperty, LineThicknessProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Brush == null) return;

            var p1 = new Point(From.X, From.Y);
            var p2 = new Point(To.X, To.Y);

            var pen = new Pen(Brush, LineThickness);
            context.DrawLine(pen, p1, p2);
        }
    }
}