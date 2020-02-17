using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Thumbs
{
    public interface IChartHaveEntityHiddableThumbs : IChartHaveHiddableThumbs
    {
        IChartThumb GetThumbByType(EntityConnectingThumbTypesEnum type);
    }
}
