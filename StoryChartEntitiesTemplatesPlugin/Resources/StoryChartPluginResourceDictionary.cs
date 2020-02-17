using System.ComponentModel.Composition;
using System.Windows;

namespace StoryChartEntitiesTemplatesPlugin.Resources
{
    [Export(typeof(ResourceDictionary))]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesDataTemplateMetadata.Name), "Story chart plugin data templates ResourceDictionary")]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesDataTemplateMetadata.Description), "Resource dictionary for story chart plugin. Plugin sample of ChartToJson")]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesDataTemplateMetadata.TakeOnlyTemplatesWhoseKeyContains), "StoryChartTemplate_")]
    public partial class StoryChartPluginResourceDictionary : ResourceDictionary
    {
        public StoryChartPluginResourceDictionary()
        {
            InitializeComponent();
        }
    }
}
