using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChartCanvasNamespace.Entities
{
    public interface IVisualWithShapeViewModel
    {
        string ShapeTemplateKey { get; set; }
        Brush ShapeStrokeBrush { get; set; }
    }
}
