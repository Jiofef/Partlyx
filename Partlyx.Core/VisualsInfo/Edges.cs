using System.Drawing;

namespace Partlyx.Core.VisualsInfo;

public record PointP(double X, double Y);
public interface IPathSegment { }
public record LineSegmentP(PointP End) : IPathSegment;
public record BezierSegmentP(PointP P1, PointP P2, PointP P3) : IPathSegment;

public class EdgeGeometryData
{
    public PointP StartPoint { get; init; }
    public List<IPathSegment> Segments { get; init; } = new();
    public PointP? TargetArrowTip { get; init; }
    public PointP? SourceArrowTip { get; init; }
}