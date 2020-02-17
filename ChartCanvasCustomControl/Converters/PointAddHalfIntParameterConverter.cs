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
    public class PointAddHalfIntParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point && parameter is int)
            {
                var p = (Point)value;
                return new Point(p.X + (((int)parameter) * 0.5d), p.Y + (((int)parameter) * 0.5d));
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point && parameter is int)
            {
                var p = (Point)value;
                return new Point(p.X - (((int)parameter) * 0.5d), p.Y - (((int)parameter) * 0.5d));
            }
            else
                throw new ArgumentException();
        }
    }
}
