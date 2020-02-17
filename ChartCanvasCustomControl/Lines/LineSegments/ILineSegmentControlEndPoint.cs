using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChartCanvasNamespace.Lines.LineSegments
{
    public interface ILineSegmentControlEndPoint : ILineSegmentBase
    {
        void ChangeEndSource(EntityConnectingThumb end);
    }
}
