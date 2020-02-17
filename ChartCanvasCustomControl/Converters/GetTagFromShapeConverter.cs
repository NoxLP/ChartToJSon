using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using NET471WPFVisualTreeHelperExtensions;

namespace ChartCanvasNamespace.Converters
{
    public class GetTagFromShapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var element = value as FrameworkElement;

            var tag = element.Tag;
            if (tag is Thickness)
                return (Thickness)tag;
            else
                return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
