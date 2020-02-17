using ChartCanvasNamespace.Lines.LineSegments;
using ChartCanvasNamespace.Thumbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Lines
{
    [Serializable]
    public class LineConnectionSaveProxy
    {
        public EntityConnectingThumbTypesEnum StartThumb;
        public EntityConnectingThumbTypesEnum EndThumb;
        public string StartVM;
        public string EndVM;
        public List<aLineSegmentProxy> Segments;
    }
}
