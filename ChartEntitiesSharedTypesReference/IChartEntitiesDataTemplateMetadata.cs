using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartToJson.MEF
{
    public interface IChartEntitiesDataTemplateMetadata
    {
        string Name { get; }
        string Description { get; }
        [DefaultValue(null)]
        string TakeOnlyTemplatesWhoseKeyContains { get; }
    }
}
