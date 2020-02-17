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
    public class LineStartSegment : aLineSegmentBase, ILineSegmentWithNextSegmentDraggerReference, IObjectWithSerializationProxy<LineStartSegmentProxy>
    {
        public LineStartSegment(LineConnection connection, LineDragger nextSegmentDragger, Binding startBinding, Point end)
            : base(LineSegmentTypesEnum.Start, connection, startBinding, end)
        {
            _Index = 0;
            _LineMathFunction = new WPFBindableTwoPointsStraightLineEquation();
            ((ILineSegmentWithNextSegmentDraggerReference)this).NextSegmentDragger = nextSegmentDragger;
            SetEquationBindings(connection);
            _LineConnecter = new LineConnecter(connection, 0);
            SetConnectersLocation();
            SetConnecterBinding(_LineConnecter);
            SetDividerBinding();
        }

        internal LineConnecter _LineConnecter;
        internal LineDragger _NextSegmentDragger;
        private WPFBindableTwoPointsStraightLineEquation _LineMathFunction;

        LineDragger ILineSegmentWithNextSegmentDraggerReference.NextSegmentDragger
        {
            get { return _NextSegmentDragger; }
            set
            {
                if (!LineConnection._LoadingFile && value == null)
                    throw new ArgumentNullException();
                if (_NextSegmentDragger == null || !_NextSegmentDragger.Equals(value))
                {
                    _NextSegmentDragger = value;
                    var b = new Binding()
                    {
                        Source = _NextSegmentDragger,
                        Path = new PropertyPath(LineDragger.AnchorPointProperty)
                    };
                    SetBinding(EndProperty, b);
                }
            }
        }

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
            ChartCustomControl.Instance.AddElementInCoordinates(
                _LineConnecter,
                p.X - (LineConnecter.Size * 0.5d),
                p.Y - (LineConnecter.Size * 0.5d));
        }
        private void SetConnecterBinding(LineConnecter connecter)
        {
            var b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(StartProperty),
                Converter = _ConnectersConverter,
                ConverterParameter = new Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>(CoordinatesEnum.X, _LineMathFunction, true),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            BindingOperations.SetBinding(_LineConnecter, Canvas.LeftProperty, b);

            b = new Binding()
            {
                Source = this,
                Path = new PropertyPath(StartProperty),
                Converter = _ConnectersConverter,
                ConverterParameter = new Tuple<CoordinatesEnum, WPFBindableTwoPointsStraightLineEquation, bool>(CoordinatesEnum.Y, _LineMathFunction, true),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            BindingOperations.SetBinding(_LineConnecter, Canvas.TopProperty, b);
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
            yield return _LineConnecter;
            yield return _LineDivider;
        }
        protected override void UpdateThumbsBingdings()
        {
            base.UpdateThumbsBingdings();
            BindingOperations.GetBindingExpression(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P0Property).UpdateTarget();
            BindingOperations.GetBindingExpression(_LineMathFunction, WPFBindableTwoPointsStraightLineEquation.P1Property).UpdateTarget();
            BindingOperations.GetBindingExpression(_LineConnecter, Canvas.LeftProperty).UpdateTarget();
            BindingOperations.GetBindingExpression(_LineConnecter, Canvas.TopProperty).UpdateTarget();
        }
        public LineStartSegmentProxy GetSerializationProxy()
        {
            return new LineStartSegmentProxy() { Start = Start, End = End };
        }
    }

    public class LineStartSegmentProxy : aLineSegmentProxy
    {
        public LineStartSegmentProxy()
        {
            Type = LineSegmentTypesEnum.Start;
            Index = 0;
        }
    }
}
