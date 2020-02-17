using ChartCanvasNamespace.Converters;
using ChartCanvasNamespace.Lines.LineSegments;
using ChartCanvasNamespace.Thumbs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFHelpers;

namespace ChartCanvasNamespace.Lines
{
    public class LineDragger : aSelectableThumbThatShowsOnMouseOver, IChartObjectCanBeRemoved, IChartObjectCanBeRemovedByParent
    {
        public LineDragger(LineConnection line, aLineSegmentBase segment, double lineMiddleX, double lineMiddleY)
        {
            _Segment = segment;
            _Connection = line;
            Width = Size;
            Height = Size;
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_LineDragger);
            DragDelta += LineDragger_DragDelta;
            DragCompleted += LineDragger_DragCompleted;
            AnchorPoint = new Point(lineMiddleX, lineMiddleY);
            ChartCustomControl.Instance.AddElementInCoordinates(this, lineMiddleX, lineMiddleY);
            var multibinding = new MultiBinding();
            multibinding.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(Canvas.LeftProperty), Mode = BindingMode.OneWay });
            multibinding.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(Canvas.TopProperty), Mode = BindingMode.OneWay });
            multibinding.Converter = _TwoDToPointConverter;
            multibinding.ConverterParameter = Size;
            SetBinding(AnchorPointProperty, multibinding);
            
            ChartCustomControl.Instance.SnapToObjectsHandler.UpdateSnapAddLineDragger(this);
            _Init = true;
            SetSelectionBrushes();
            ToolTip = Properties.ToolTips.Default.ToolTips_LineDragger;
        }

        #region fields
        private static object _LockObject = new object();
        private static SolidColorBrush _SelectedBrush;
        //private static SolidColorBrush _NotSelectedBackgroundBrush;
        private static SolidColorBrush _NotSelectedForegroundBrush;
        private static TwoDoublesToPointHalfSizeParameterConverter _TwoDToPointConverter = new TwoDoublesToPointHalfSizeParameterConverter();
        private bool _Init;
        internal LineConnection _Connection;
        internal aLineSegmentBase _Segment;
        #endregion

        public static int Size { get { return 15; } }

        public Point AnchorPoint
        {
            get { return (Point)GetValue(AnchorPointProperty); }
            set { SetValue(AnchorPointProperty, value); }
        }
        public static readonly DependencyProperty AnchorPointProperty =
            DependencyProperty.Register("AnchorPoint", typeof(Point), typeof(LineDragger), new PropertyMetadata(default(Point), OnAnchorPointPropertyChanged));
        private static void OnAnchorPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LineDragger)d).OnAnchorPointChanged((Point)e.NewValue, (Point)e.OldValue);
        }
        private void OnAnchorPointChanged(Point newPoint, Point oldPoint)
        {
            if (!_Init)
                return;

            ChartCustomControl.Instance.SnapToObjectsHandler.UpdateSnapRemoveLineDragger(oldPoint, this);
            ChartCustomControl.Instance.SnapToObjectsHandler.UpdateSnapAddLineDragger(this);
        }

        #region drag drop
        private Point _UndoMovingCoordinates;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            _UndoMovingCoordinates = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
        }
        private void LineDragger_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ChartCustomControl.Instance.SnapToObjectsHandler.HidSnapLines();
            var parameters = new object[3] { this, _UndoMovingCoordinates, new Point(Canvas.GetLeft(this), Canvas.GetTop(this)) };
            UndoRedoSystem.UndoRedoCommandManager.Instance.NewCommand(
                Properties.UndoRedoNames.Default.LineConnection_MoveLineDragger, 
                x => ChartCustomControl.Instance.MoveElementToCoordinates((LineDragger)x[0], ((Point)x[1]).X, ((Point)x[1]).Y),
                parameters,
                x => ChartCustomControl.Instance.MoveElementToCoordinates((LineDragger)x[0], ((Point)x[2]).X, ((Point)x[2]).Y),
                parameters);
        }
        private void LineDragger_DragDelta(object sender, DragDeltaEventArgs e)
        {
            _Connection.HideAllThumbs();
            var canvas = ChartCustomControl.Instance;
            Point p = new Point(Canvas.GetLeft(this) + e.HorizontalChange, Canvas.GetTop(this) + e.VerticalChange);

            var snap = canvas.SnapToObjectsHandler.CheckLineDraggerPointShouldSnap(p, this);

            if (snap != null)
            {
                if (snap.X.HasValue)
                    p.X = snap.X.Value;
                if (snap.Y.HasValue)
                    p.Y = snap.Y.Value;
            }

            Canvas.SetLeft(this, p.X - ActualWidth / 2);
            Canvas.SetTop(this, p.Y - ActualHeight / 2);

            e.Handled = true;
        }
        #endregion

        private void SetSelectionBrushes()
        {
            if (_SelectedBrush == null)
            {
                lock (_LockObject)
                {
                    if (_SelectedBrush == null)
                    {
                        //var resourceDict = new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/ChartCanvas;component/Resources/ResDict0.xaml") };

                        _SelectedBrush = (SolidColorBrush)Application.Current.FindResource("SelectionLineSelectedBrush");
                    }
                    if (_NotSelectedForegroundBrush == null)
                    {
                        //_NotSelectedBackgroundBrush = Background as SolidColorBrush;
                        _NotSelectedForegroundBrush = (SolidColorBrush)Application.Current.FindResource("LineDraggerSelectionBorderUnSelectedBrush");
                    }
                }
            }
        }
        protected override void UpdateSelectedVisualEffect()
        {
            if (IsSelected)
            {
                //Background = _SelectedBrush;
                Foreground = _SelectedBrush;
            }
            else
            {
                //Background = _NotSelectedBackgroundBrush;
                Foreground = _NotSelectedForegroundBrush;
            }
        }

        #region remove
        public bool AlreadyRemovedByParent { get; set; }

        public void RemoveThis()
        {
            if (AlreadyRemovedByParent)
                return;
            _Connection.RemoveSegment(this);
        }
        public void AddToLastCommandMyUndoRedoCommands()
        {
            var paramet = new object[1] { this };
            UndoRedoSystem.UndoRedoCommandManager.Instance.AddToLastCommand(
                x => ((LineDragger)x[0]).AlreadyRemovedByParent = false,
                paramet,
                null,
                null);
        }
        public void RemovedByParent()
        {
            AlreadyRemovedByParent = true;
        }
        #endregion
    }
}
