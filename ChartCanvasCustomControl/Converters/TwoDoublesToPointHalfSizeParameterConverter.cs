using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChartCanvasNamespace.Converters
{
    public class TwoDoublesToPointHalfSizeParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var doubles = values.Cast<double>().ToArray();
            var halfSize = parameter != null ? ((int)parameter) * 0.5d : 0d;

            return new Point(doubles[0], doubles[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
