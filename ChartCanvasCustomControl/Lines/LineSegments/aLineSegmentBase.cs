using ChartCanvasNamespace.Converters;
using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Thumbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ChartCanvasNamespace.Lines.LineSegments
{
    public abstract class aLineSegmentBase : FrameworkElement, IChartHaveHiddableThumbs, ILineSegmentBase
    {
        #region constructors
        public aLineSegmentBase(LineSegmentTypesEnum Type, LineConnection connection, Binding startBinding, Binding endBinding)
        {
            _Connection = connection;
            _Type = Type;
            _LineDivider = new LineDivider(connection, this);
            SetBinding(StartProperty, startBinding);
            SetBinding(EndProperty, endBinding);

            SetDividerBinding();
            _Loaded = true;
        }
        public aLineSegmentBase(LineSegmentTypesEnum Type, LineConnection connection, Point start, Binding endBinding)
        {
            _Connection = connection;
            _Type = Type;
            _LineDivider = new LineDivider(connection, this);
            Start = start;
            SetBinding(EndProperty, endBinding);

            _Loaded = true;
        }
        public aLineSegmentBase(LineSegmentTypesEnum Type, LineConnection connection, Binding startBinding, Point end)
        {
            _Connection = connection;
            _Type = Type;
            _LineDivider = new LineDivider(connection, this);
            SetBinding(StartProperty, startBinding);
            End = end;

            _Loaded = true;
        }
        public aLineSegmentBase(LineSegmentTypesEnum Type, LineConnection connection, EntityConnectingThumb start, EntityConnectingThumb end)
        {
            _Connection = connection;
            _Type = Type;
            _LineDivider = new LineDivider(connection, this);
            Start = start.AnchorPoint;
            End = end.AnchorPoint;
            var b = new Binding()
            {
                Source = start,
                Path = new PropertyPath(EntityConnectingThumb.AnchorPointProperty),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            SetBinding(StartProperty, b);
            b = new Binding()
            {
                Source = end,
                Path = new PropertyPath(EntityConnectingThumb.AnchorPointProperty),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            SetBinding(EndProperty, b);

            SetDividerBinding();
            _Loaded = true;
        }
        public aLineSegmentBase(LineSegmentTypesEnum Type, LineConnection connection, Point start, Point end)
        {
            _Connection = connection;
            _Type = Type;
            _LineDivider = new LineDivider(connection, this);
            Start = start;
            End = end;

            SetDividerBinding();
            _Loaded = true;
        }
        protected void SetDividerBinding()
        {
            var multiBinding = new MultiBinding();
            multiBinding.Converter = new TwoPointsMiddlePointSubtractHalfSizeParameterConverter();
            multiBinding.ConverterParameter = LineDivider.Size;
            multiBinding.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(aLineSegmentBase.StartProperty) });
            multiBinding.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(aLineSegmentBase.EndProperty) });
            multiBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            SetBinding(aLineSegmentBase.MiddlePointProperty, multiBinding);
            ChartCustomControl.Instance.AddElementInCoordinates(_LineDivider, MiddlePoint.X, MiddlePoint.Y);
            _LineDivider.SetBinding(Canvas.LeftProperty, new Binding("MiddlePoint.X") { Source = this });
            _LineDivider.SetBinding(Canvas.TopProperty, new Binding("MiddlePoint.Y") { Source = this });
            BindingOperations.GetMultiBindingExpression(this, MiddlePointProperty).UpdateSource();
        }
        #endregion

        #region fields
        protected static PointAddHalfIntParameterConverter _PointHalfParameterConverter = new PointAddHalfIntParameterConverter();
        protected static LineConnecterPositionConverter _ConnectersConverter = new LineConnecterPositionConverter();
        internal LineSegmentTypesEnum _Type;
        internal LineDivider _LineDivider;
        private LineConnection _Connection;
        private bool _Loaded;
        public int _Index;
        #endregion

        #region dependency properties
        public Point Start
        {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(aLineSegmentBase), new PropertyMetadata(default(Point), OnPointPropChanged));
        public Point End
        {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(aLineSegmentBase), new PropertyMetadata(default(Point), OnPointPropChanged));
        public Point MiddlePoint
        {
            get { return (Point)GetValue(MiddlePointProperty); }
            set { SetValue(MiddlePointProperty, value); }
        }
        public static readonly DependencyProperty MiddlePointProperty =
            DependencyProperty.Register("MiddlePoint", typeof(Point), typeof(aLineSegmentBase), new PropertyMetadata(default(Point)));
        private static void OnPointPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine($"Static Line start changed");
            ((aLineSegmentBase)d).OnPointChanged((Point)e.NewValue);
        }
        private void OnPointChanged(Point p)
        {
            Console.WriteLine($"Line start changed= {p}");
            if (!_Loaded)
                return;
            _Connection.InvalidateMeasure();
        }
        #endregion

        public void ChangeStartSource(EntityConnectingThumb start)
        {
            var b = new Binding()
            {
                Source = start,
                Path = new PropertyPath(EntityConnectingThumb.AnchorPointProperty),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            SetBinding(StartProperty, b);
        }
        public void ChangeStartSource(LineDragger dragger)
        {
            var b = new Binding()
            {
                Source = dragger,
                Path = new PropertyPath(LineDragger.AnchorPointProperty),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            SetBinding(StartProperty, b);
        }

        protected bool _ThumbsHidden;
        public virtual void HideAllThumbs()
        {
            if (_ThumbsHidden)
                return;
            DoActionAllThumbs(x =>
            {
                if(!x.IsMouseOver)
                    x.Visibility = Visibility.Hidden;
            });
            _ThumbsHidden = true;
        }
        public void ShowAllThumbs()
        {
            if (!_ThumbsHidden || IsMouseOverAnyThumb())
                return;

            UpdateThumbsBingdings();
            DoActionAllThumbs(x => x.Visibility = Visibility.Visible);
            _ThumbsHidden = false;
        }
        public abstract IEnumerable<IChartThumb> GetAllThumbs();
        public virtual void DoActionAllThumbs(Action<IChartThumb> action)
        {
            foreach (var item in GetAllThumbs())
            {
                action(item);
            }
        }
        protected virtual void UpdateThumbsBingdings()
        {
            BindingOperations.GetMultiBindingExpression(this, MiddlePointProperty).UpdateSource();
            BindingOperations.GetMultiBindingExpression(this, MiddlePointProperty).UpdateTarget();
        }
        public void UpdateStartEndBindings()
        {
            Console.WriteLine($"Update start/end bindings ; NON updated start = {Start}");
            var sThumb = (EntityConnectingThumb)BindingOperations.GetBindingExpression(this, StartProperty).ResolvedSource;
            Console.WriteLine($"Updated anchor point = {sThumb.AnchorPoint}");
            Start = new Point(sThumb.AnchorPoint.X, sThumb.AnchorPoint.Y);
            Console.WriteLine($"Updated start = {Start}");
            BindingOperations.GetBindingExpression(this, StartProperty).UpdateSource();
            BindingOperations.GetBindingExpression(this, StartProperty).UpdateTarget();
            BindingOperations.GetBindingExpression(this, EndProperty).UpdateSource();
            BindingOperations.GetBindingExpression(this, EndProperty).UpdateSource();
        }

        public void RemoveAllThumbsFromCanvas()
        {
            DoActionAllThumbs(x =>
            {
                var selectable = x as Selection.IVisualCanBeSelected;
                if (selectable != null && selectable.IsSelected)
                {
                    selectable.IsSelected = false;
                }

                ChartCustomControl.Instance.ChartCanvas.Children.Remove(x as UIElement);
            });
        }
        public void AddAllThumbsToCanvas()
        {
            DoActionAllThumbs(x =>
            {
                ChartCustomControl.Instance.ChartCanvas.Children.Add(x as UIElement);
            });
            UpdateThumbsBingdings();
        }
        public bool IsMouseOverAnyThumb()
        {
            return GetAllThumbs().Any(x => x.IsMouseOver);
        }
    }

    public abstract class aLineSegmentProxy
    {
        public int Index;
        public Point Start;
        public Point End;
        public LineSegmentTypesEnum Type;
    }
}
