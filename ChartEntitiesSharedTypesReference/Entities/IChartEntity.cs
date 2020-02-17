using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Entities
{
    public interface IChartEntity
    {
        string EntityId { get; }

        bool NewChildAddedFromChart(IChartEntity entity);
        bool RemoveChild(IChartEntity entity);
    }
}
