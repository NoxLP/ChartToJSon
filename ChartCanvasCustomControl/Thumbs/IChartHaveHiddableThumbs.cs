using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Thumbs
{
    public interface IChartHaveHiddableThumbs
    {
        void HideAllThumbs();
        void ShowAllThumbs();
        IEnumerable<IChartThumb> GetAllThumbs();
        void DoActionAllThumbs(Action<IChartThumb> action);
    }
}
