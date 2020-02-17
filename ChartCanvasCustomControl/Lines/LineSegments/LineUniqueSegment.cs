using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Thumbs;
using JsonManagerLibrary;
using Math471.StraightLine;
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
    public class LineUniqueSegment : aLineSegmentBase, ILineSegmentControlEndPoint, IObjectWithSerializationProxy<LineUniqueSegmentProxy>
    {
        public LineUniqueSegment(LineConnection connection, EntityConnectingThumb start, EntityConnectingThumb end)
            : base(LineSegmentTypesEnum.Unique, connection, start, end)
        {
            _Index = 0;
            _LineMathFunction = new WPFBindableTwoPointsStraightLineEquation();
            SetEquationBindings(connection);

            _StartLineConnecter = new LineConnecter(connection, 0);
            _EndLineConnecter = new LineConnecter(connection, 1);
            SetConnectersLocation();
            SetConnecterBinding(_StartLineConnecter, true);
            SetConnecterBinding(_EndLineConnecter, false);
        }

        internal LineConnecter _StartLineConnecter;
        internal LineConnecter _EndLineConnecter;
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
            var p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Start, margin);
            var canvas = ChartCustomControl.Instance;
            canvas.AddElementInCoordinates(
                _StartLineConnecter,
                p.X - (LineConnecter.Size * 0.5d),
                p.Y - (LineConnecter.Size * 0.5d));
            p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(End, -margin);
            canvas.AddElementInCoordinates(
                _EndLineConnecter,
                p.X - (LineConnecter.Size * 0.5d),
                p.Y - (LineConnecter.Size * 0.5d));
        }
        private void SetConnecterBinding(LineConnecter connecter, bool start)
        {
            var b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(start ? aLineSegmentBase.StartProperty : aLineSegmentBase.EndProperty),
                Converter = _ConnectersConverter,
                ConverterParameter = new Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>(CoordinatesEnum.X, _LineMathFunction, start),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(start ? _StartLineConnecter : _EndLineConnecter, Canvas.LeftProperty, b);

            b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(start ? aLineSegmentBase.StartProperty : aLineSegmentBase.EndProperty),
                Converter = _ConnectersConverter,
                ConverterParameter = new Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>(CoordinatesEnum.Y, _LineMathFunction, start),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(start ? _StartLineConnecter : _EndLineConnecter, Canvas.TopProperty, b);
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
            yield return _StartLineConnecter;
            yield return _LineDivider;
            yield return _EndLineConnecter;
            yield break;
        }
        protected override void UpdateThumbsBingdings()
        {
            base.UpdateThumbsBingdings();
            BindingOperations.GetBindingExpression(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P0Property).UpdateSource();
            BindingOperations.GetBindingExpression(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P1Property).UpdateSource();
            BindingOperations.GetBindingExpression(_StartLineConnecter, Canvas.LeftProperty).UpdateSource();
            BindingOperations.GetBindingExpression(_StartLineConnecter, Canvas.LeftProperty).UpdateSource();
            BindingOperations.GetBindingExpression(_EndLineConnecter, Canvas.LeftProperty).UpdateSource();
            BindingOperations.GetBindingExpression(_EndLineConnecter, Canvas.LeftProperty).UpdateSource();
        }
        public LineUniqueSegmentProxy GetSerializationProxy()
        {
            return new LineUniqueSegmentProxy() { Start = Start, End = End };
        }
    }

    public class LineUniqueSegmentProxy : aLineSegmentProxy
    {
        public LineUniqueSegmentProxy()
        {
            Type = LineSegmentTypesEnum.Unique;
            Index = 0;
        }
    }
}
