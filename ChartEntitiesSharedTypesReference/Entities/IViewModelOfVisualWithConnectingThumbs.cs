using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Entities
{
    public interface IViewModelOfVisualWithConnectingThumbs
    {
        double CanvasX { get; set; }
        double CanvasY { get; set; }
        string ViewModelId { get; set; }
    }
}
