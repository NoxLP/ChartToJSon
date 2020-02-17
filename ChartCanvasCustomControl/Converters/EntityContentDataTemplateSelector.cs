using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChartCanvasNamespace.Converters
{
    public class EntityContentDataTemplateSelector : DataTemplateSelector
    {
        private static Dictionary<string, DataTemplate> _Templates = new Dictionary<string, DataTemplate>();
        public DataTemplate DefaultTemplate { get; set; }

        public static void AddTemplates(Dictionary<string, DataTemplate> templates)
        {
            foreach (var item in templates)
            {
                _Templates.Add(item.Key, item.Value);
            }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var vm = item as IChartEntityViewModel;
            if (vm == null || _Templates == null || !_Templates.ContainsKey(vm.TemplateKey))
                return DefaultTemplate;

            return _Templates[vm.TemplateKey];
        }
    }
}
