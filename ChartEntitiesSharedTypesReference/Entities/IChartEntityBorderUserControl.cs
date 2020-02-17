using ChartCanvasNamespace.Entities.EntitiesShapesUserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Entities
{
    public interface IChartEntityBorderUserControl
    {
        ChartEntityUserControl EntityUserControl { get; set; }
    }
}
