using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Lines.LineSegments
{
    public interface ILineSegmentWithDragger : ILineSegmentBase
    {
        LineDragger MyLineDragger { get; }
    }
}
