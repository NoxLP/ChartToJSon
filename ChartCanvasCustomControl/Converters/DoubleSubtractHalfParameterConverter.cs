using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChartCanvasNamespace.Converters
{
    public class DoubleSubtractHalfParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double && parameter is double)
            {
                return (double)value - (((double)parameter) * 0.5d);
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double && parameter is double)
            {
                return (double)value + (((double)parameter) * 0.5d);
            }
            else
                throw new ArgumentException();
        }
    }
}
