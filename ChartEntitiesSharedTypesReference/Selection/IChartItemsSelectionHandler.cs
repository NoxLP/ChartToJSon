using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Selection
{
    public interface IChartItemsSelectionHandler
    {
        void ItemSelected(IVisualCanBeSelected selected);
    }
}
