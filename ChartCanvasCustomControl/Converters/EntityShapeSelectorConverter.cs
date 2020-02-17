using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using WPFHelpers;

namespace ChartCanvasNamespace.Converters
{
    public class EntityShapeSelectorConverter : IValueConverter
    {
        private static Dictionary<string, Shape> _Shapes = new Dictionary<string, Shape>();
        public DataTemplate DefaultTemplate { get; set; }

        public static void AddTemplates(Dictionary<string, Shape> templates)
        {
            foreach (var item in templates)
            {
                _Shapes.Add(item.Key, item.Value);
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var key = value as string;
            if (string.IsNullOrEmpty(key) || !_Shapes.ContainsKey(key))
                return DefaultTemplate;

            return _Shapes[key].XamlDeepClone();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
