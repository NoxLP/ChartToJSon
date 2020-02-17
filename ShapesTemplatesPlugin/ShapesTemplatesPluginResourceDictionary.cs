using System.ComponentModel.Composition;
using System.Windows;

namespace ShapesTemplatesPlugin
{
    [Export(typeof(ResourceDictionary))]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesDataTemplateMetadata.Name), "Default shapes plugin data templates ResourceDictionary")]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesDataTemplateMetadata.Description), "Resource dictionary for default shapes data templates. Plugin sample of ChartToJson")]
    public partial class ShapesTemplatesPluginResourceDictionary : ResourceDictionary
    {
        public ShapesTemplatesPluginResourceDictionary()
        {
            InitializeComponent();
        }
    }
}
