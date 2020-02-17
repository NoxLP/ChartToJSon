using System.Windows;
using ChartCanvasNamespace.Entities;

namespace ChartCanvasNamespace.Lines.LineSegments
{
    public interface ILineSegmentBase
    {
        Point End { get; set; }
        Point MiddlePoint { get; set; }
        Point Start { get; set; }

        void ChangeStartSource(EntityConnectingThumb start);
        void ChangeStartSource(LineDragger dragger);
        bool IsMouseOverAnyThumb();
        void RemoveAllThumbsFromCanvas();
        void AddAllThumbsToCanvas();
    }
}