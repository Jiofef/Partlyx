using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Partlyx.Core.VisualsInfo;
using System.Collections.Generic;

namespace Partlyx.ViewModels.Graph;

public static class MsaglMapper
{
    public static EdgeGeometryData ToPartlyxGeometry(this Edge msaglEdge)
    {
        var geom = msaglEdge.EdgeGeometry;
        var curve = geom.Curve;

        var data = new EdgeGeometryData
        {
            StartPoint = new PointP(curve.Start.X, -curve.Start.Y),
            TargetArrowTip = geom.TargetArrowhead != null 
                ? new PointP(geom.TargetArrowhead.TipPosition.X, -geom.TargetArrowhead.TipPosition.Y) 
                : null,
            SourceArrowTip = geom.SourceArrowhead != null 
                ? new PointP(geom.SourceArrowhead.TipPosition.X, -geom.SourceArrowhead.TipPosition.Y) 
                : null
        };

        ProcessCurve(curve, data.Segments);
        return data;
    }

    private static void ProcessCurve(ICurve curve, List<IPathSegment> segments)
    {
        switch (curve)
        {
            case Curve composite:
                foreach (var s in composite.Segments) ProcessCurve(s, segments);
                break;

            case LineSegment line:
                segments.Add(new LineSegmentP(new PointP(line.End.X, -line.End.Y)));
                break;

            case Polyline poly:
                var p = poly.StartPoint;
                while (p.Next != null)
                {
                    segments.Add(new LineSegmentP(new PointP(p.Next.Point.X, p.Next.Point.Y)));
                    p = p.Next;
                }
                break;

            case CubicBezierSegment b:
                segments.Add(new BezierSegmentP(
                    new PointP(b.B(1).X, -b.B(1).Y),
                    new PointP(b.B(2).X, -b.B(2).Y),
                    new PointP(b.B(3).X, -b.B(3).Y)));
                break;
        }
    }
}