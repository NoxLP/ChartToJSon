using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Thumbs;
using JsonManagerLibrary;
using Math471.StraightLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ChartCanvasNamespace.Lines.LineSegments
{
    public class LineEndSegment : aLineSegmentBase, ILineSegmentControlEndPoint, ILineSegmentWithDragger, IObjectWithSerializationProxy<LineEndSegmentProxy>
    {
        public LineEndSegment(LineConnection connection, Point start, Binding endBinding, int index)
            : base(LineSegmentTypesEnum.End, connection, start, endBinding)
        {
            _Index = index;
            _LineMathFunction = new WPFBindableTwoPointsStraightLineEquation();
            SetEquationBindings(connection);

            _LineConnecter = new LineConnecter(connection, 1);
            SetConnectersLocation();
            SetConnecterBinding(_LineConnecter);

            MyLineDragger = new LineDragger(connection, this, start.X, start.Y);
            var b = new Binding()
            {
                Source = MyLineDragger,
                Path = new PropertyPath(LineDragger.AnchorPointProperty),
                Converter = _PointHalfParameterConverter,
                ConverterParameter = LineDragger.Size
            };
            SetBinding(StartProperty, b);
            SetDividerBinding();
        }

        public LineDragger MyLineDragger { get; private set; }
        internal LineConnecter _LineConnecter;
        private WPFBindableTwoPointsStraightLineEquation _LineMathFunction;

        private void SetEquationBindings(LineConnection connection)
        {
            var b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(StartProperty),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            BindingOperations.SetBinding(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P0Property, b);
            b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(EndProperty),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            BindingOperations.SetBinding(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P1Property, b);
        }
        private void SetConnectersLocation()
        {
            var margin = Properties.Settings.Default.LinesConnectersYMargin;// _LineMathFunction.Slope > 0 ? Properties.Settings.Default.LinesConnectersYMargin : -1 * Properties.Settings.Default.LinesConnectersYMargin;
            var p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(End, -margin);
            var canvas = ChartCustomControl.Instance;
            canvas.AddElementInCoordinates(
                _LineConnecter,
                p.X - (LineConnecter.Size * 0.5d),
                p.Y - (LineConnecter.Size * 0.5d));
        }
        private void SetConnecterBinding(LineConnecter connecter)
        {
            var b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(EndProperty),
                Converter = _ConnectersConverter,
                ConverterParameter = new Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>(CoordinatesEnum.X, _LineMathFunction, false),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            BindingOperations.SetBinding(_LineConnecter, Canvas.LeftProperty, b);

            b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(EndProperty),
                Converter = _ConnectersConverter,
                ConverterParameter = new Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>(CoordinatesEnum.Y, _LineMathFunction, false),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            BindingOperations.SetBinding(_LineConnecter, Canvas.TopProperty, b);
        }

        public void ChangeEndSource(EntityConnectingThumb end)
        {
            var b = new Binding()
            {
                Source = end,
                Path = new PropertyPath(EntityConnectingThumb.AnchorPointProperty),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            SetBinding(EndProperty, b);
        }

        public override void HideAllThumbs()
        {
            if (_ThumbsHidden)
                return;
            DoActionAllThumbs(x =>
            {
                var connecter = x as LineConnecter;
                if (connecter == null || (connecter.IsChecked.HasValue && !connecter.IsChecked.Value))
                {
                    if (!x.IsMouseOver)
                        x.Visibility = Visibility.Hidden;
                }
            });
            _ThumbsHidden = true;
        }
        public override IEnumerable<IChartThumb> GetAllThumbs()
        {
            yield return MyLineDragger;
            yield return _LineDivider;
            yield return _LineConnecter;
            yield break;
        }
        protected override void UpdateThumbsBingdings()
        {
            base.UpdateThumbsBingdings();
            BindingOperations.GetBindingExpression(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P0Property).UpdateTarget();
            BindingOperations.GetBindingExpression(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P1Property).UpdateTarget();
            BindingOperations.GetBindingExpression(_LineConnecter, Canvas.LeftProperty).UpdateTarget();
            BindingOperations.GetBindingExpression(_LineConnecter, Canvas.TopProperty).UpdateTarget();
        }
        public LineEndSegmentProxy GetSerializationProxy()
        {
            return new LineEndSegmentProxy() { Start = Start, End = End, Index = _Index };
        }
    }

    public class LineEndSegmentProxy : aLineSegmentProxy
    {
        public LineEndSegmentProxy()
        {
            Type = LineSegmentTypesEnum.End;
        }

        public int Index;
    }
}
