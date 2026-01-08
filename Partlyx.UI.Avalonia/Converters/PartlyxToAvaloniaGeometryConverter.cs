using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Partlyx.Core.VisualsInfo;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Partlyx.UI.Avalonia.Converters
{
    /// <summary>
    /// Converts neutral EdgeGeometryData into Avalonia-compatible StreamGeometry.
    /// This ensures the View remains decoupled from the specific graph layout engine.
    /// </summary>
    public class PartlyxToAvaloniaGeometryConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 'value' is the EdgeGeometryData from our ViewModel
            if (value is not EdgeGeometryData data)
                return null;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                // 1. Initialize the path at the start point
                Point lastPoint = new Point(data.StartPoint.X, data.StartPoint.Y);
                context.BeginFigure(lastPoint, false);

                // 2. Iterate through segments (Lines or Beziers)
                foreach (var segment in data.Segments)
                {
                    if (segment is LineSegmentP line)
                    {
                        lastPoint = new Point(line.End.X, line.End.Y);
                        context.LineTo(lastPoint);
                    }
                    else if (segment is BezierSegmentP bezier)
                    {
                        lastPoint = new Point(bezier.P3.X, bezier.P3.Y);
                        context.CubicBezierTo(
                            new Point(bezier.P1.X, bezier.P1.Y),
                            new Point(bezier.P2.X, bezier.P2.Y),
                            lastPoint);
                    }
                }

                context.EndFigure(false);

                // 3. Render Arrowhead at Target if data exists
                // In MSAGL, the curve usually ends at the base of the arrowhead
                if (data.TargetArrowTip != null)
                {
                    DrawArrowhead(context, lastPoint, new Point(data.TargetArrowTip.X, data.TargetArrowTip.Y));
                }

                // 4. Render Arrowhead at Source (for bidirectional edges)
                if (data.SourceArrowTip != null)
                {
                    // For the source, the 'start' of the curve is the base for this arrowhead
                    Point curveStart = new Point(data.StartPoint.X, data.StartPoint.Y);
                    DrawArrowhead(context, curveStart, new Point(data.SourceArrowTip.X, data.SourceArrowTip.Y));
                }
            }
            return geometry;
        }

        /// <summary>
        /// Draws a triangular arrowhead from the edge boundary to the tip position.
        /// </summary>
        private void DrawArrowhead(StreamGeometryContext context, Point edgeEndpoint, Point tip)
        {
            // Vector representing the direction of the arrow
            Vector direction = tip - edgeEndpoint;
            double length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (length > 0)
            {
                Vector unitVector = direction / length;
                // Orthogonal vector for the width of the arrow wings
                Vector sideVector = new Vector(-unitVector.Y, unitVector.X) * 3;
                // Move slightly back from the tip to create the base of the triangle
                Point basePoint = tip - (unitVector * 5);

                context.BeginFigure(tip, true); // Create a filled shape
                context.LineTo(basePoint + sideVector);
                context.LineTo(basePoint - sideVector);
                context.EndFigure(true);
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }
}