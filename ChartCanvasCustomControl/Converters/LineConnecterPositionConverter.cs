using ChartCanvasNamespace.Lines;
using Math471.StraightLine;
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
    public class LineConnecterPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = parameter as Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>;
            if (parameters == null)
                throw new ArgumentException();

            if (value is Point)
            {
                var margin = Properties.Settings.Default.LinesConnectersYMargin;
                var p = parameters.Item2.GetPointAlongLineAtDistanceFrom((Point)value, parameters.Item3 ? margin : -margin);
                double coord = parameters.Item1 == CoordinatesEnum.X ? p.X : p.Y;
                return coord - (LineConnecter.Size * 0.5d);
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
