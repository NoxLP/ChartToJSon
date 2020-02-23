using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Lines.LineSegments;
using ChartCanvasNamespace.Thumbs;
using JsonManagerLibrary;
using Math471.StraightLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using UndoRedoSystem;
using UndoRedoSystem.UndoRedoCommands;

namespace ChartCanvasNamespace.Lines
{
    public class LineConnection : aChartArrowLineBase, IChartObjectCanBeRemoved, IChartObjectCanBeRemovedByParent, IChartHaveHiddableThumbs, IObjectWithSerializationProxy<LineConnectionSaveProxy>
    {
        public LineConnection(EntityConnectingThumb startThumb, EntityConnectingThumb endThumb)
        {
            SetLineSelectedBrush();
            StartThumb = startThumb;
            EndThumb = endThumb;

            IsHitTestVisible = false;
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_LinePath);
            _HitboxPolyLine = new Polyline();
            _HitboxPolyLine.Name = "LineConnectionHitbox";
            _HitboxPolyLine.Stroke = Brushes.Transparent;
            _HitboxPolyLine.StrokeThickness = 8;
            _HitboxPolyLine.IsHitTestVisible = true;
            _HitboxPolyLine.Tag = this;
            _HitboxPolyLine.IsMouseDirectlyOverChanged += HitboxOnIsMouseDirectlyOverChanged;
            ChartCustomControl.Instance.ChartCanvas.Children.Add(_HitboxPolyLine);
            Panel.SetZIndex(_HitboxPolyLine, Properties.Settings.Default.ZIndex_LineHitBox);
            _HitboxPolyLine.PreviewMouseDown += HitBoxPreviewMouseDown;
            _HitboxPolyLine.PreviewMouseUp += HitBoxPreviewMouseUp;
            _HitboxPolyLine.ToolTip = Properties.ToolTips.Default.ToolTips_LineConnection;

            Segments = new ObservableCollection<aLineSegmentBase>();
            Segments.Add(new LineUniqueSegment(this, startThumb, endThumb));
            ChartCustomControl.Instance.ChartCanvas.Children.Add(this);
            HideAllThumbs();
            Stroke = Brushes.Black;
            StrokeThickness = 1.0d;

            var undoParams = new object[3] { this, StartThumb, EndThumb };
            var redoParams = new object[1] { this };
            UndoRedoCommandManager.Instance.NewCommand(
                Properties.UndoRedoNames.Default.LineConnection_CreateLine,
                RemoveConnectionRedoAction,
                redoParams,
                RemoveConnectionUndoAction,
                undoParams);
        }
        public LineConnection(EntityConnectingThumb startThumb, EntityConnectingThumb endThumb, List<aLineSegmentProxy> segmentProxies)
        {
            SetLineSelectedBrush();
            StartThumb = startThumb;
            EndThumb = endThumb;

            IsHitTestVisible = false;
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_LinePath);
            _HitboxPolyLine = new Polyline();
            _HitboxPolyLine.Stroke = Brushes.Transparent;
            _HitboxPolyLine.StrokeThickness = 8;
            _HitboxPolyLine.IsHitTestVisible = true;
            _HitboxPolyLine.Tag = this;
            _HitboxPolyLine.IsMouseDirectlyOverChanged += HitboxOnIsMouseDirectlyOverChanged;
            ChartCustomControl.Instance.ChartCanvas.Children.Add(_HitboxPolyLine);
            Panel.SetZIndex(_HitboxPolyLine, Properties.Settings.Default.ZIndex_LineHitBox);
            _HitboxPolyLine.PreviewMouseDown += HitBoxPreviewMouseDown;
            _HitboxPolyLine.PreviewMouseUp += HitBoxPreviewMouseUp;
            _HitboxPolyLine.ToolTip = Properties.ToolTips.Default.ToolTips_LineConnection;

            Segments = new ObservableCollection<aLineSegmentBase>();
            //Segments.Add(new LineUniqueSegment(this, startThumb, endThumb));
            for (int i = segmentProxies.Count - 1; i >= 0; i--)
            {
                var item = segmentProxies[i];
                aLineSegmentBase segment;
                switch(item.Type)
                {
                    case LineSegmentTypesEnum.Unique:
                        segment = new LineUniqueSegment(this, startThumb, endThumb);
                        break;
                    case LineSegmentTypesEnum.Start:
                        var pp = new PropertyPath(EntityConnectingThumb.AnchorPointProperty);
                        var sb = new Binding() { Source = StartThumb, Path = pp };
                        segment = new LineStartSegment(this, null, sb, item.End);
                        break;
                    case LineSegmentTypesEnum.Normal:
                        segment = new LineNormalSegment(this, item.Start, item.End, null, ((LineNormalSegmentProxy)item).Index);
                        break;
                    default://case LineSegmentTypesEnum.End:
                        pp = new PropertyPath(EntityConnectingThumb.AnchorPointProperty);
                        var eb = new Binding() { Source = EndThumb, Path = pp };
                        segment = new LineEndSegment(this, item.Start, eb, ((LineEndSegmentProxy)item).Index);
                        break;
                }

                Segments.Insert(item.Index, segment);

                if (segment is ILineSegmentWithNextSegmentDraggerReference && i != (segmentProxies.Count - 1))
                    ((ILineSegmentWithNextSegmentDraggerReference)segment).NextSegmentDragger = ((ILineSegmentWithDragger)Segments[i + 1]).MyLineDragger;
            }
            ChartCustomControl.Instance.ChartCanvas.Children.Add(this);
            HideAllThumbs();
            Stroke = Brushes.Black;
            StrokeThickness = 1.0d;
            InvalidateMeasure();
        }

        #region fields
        private static object _LockObject = new object();
        private static SolidColorBrush _SelectedBrush = null;
        private Polyline _HitboxPolyLine;
        public static bool _LoadingFile;
        private EntityConnectingThumb _StartThumb;
        private EntityConnectingThumb _EndThumb;
        #endregion

        #region properties
        public EntityConnectingThumb StartThumb
        {
            get { return _StartThumb; }
            set
            {
                if (value == null)
                {
                    if (_StartThumb != null)
                    {
                        if (!_LoadingFile && _EndThumb != null)
                        {
                            var startBorderVM = _StartThumb.MyEntityViewModel as IChartEntityViewModel;
                            var endBorderVM = _EndThumb != null ? _EndThumb.MyEntityViewModel as IChartEntityViewModel : null;
                            if (startBorderVM != null && endBorderVM != null)
                                startBorderVM.RemoveChild(endBorderVM);
                            else
                                _EndThumb._LinesEnds.Remove(this);
                            _StartThumb._LinesStarts.Remove(this);
                        }
                        _StartThumb = null;
                    }
                }
                else if (!value.Equals(_StartThumb))
                {
                    if (!_LoadingFile && _EndThumb != null)
                    {
                        var startBorderVM = _StartThumb.MyEntityViewModel as IChartEntityViewModel;
                        var endBorderVM = _EndThumb.MyEntityViewModel as IChartEntityViewModel;
                        if (startBorderVM != null && endBorderVM != null)
                        {
                            startBorderVM.RemoveChild(endBorderVM);
                        }
                        else
                            _StartThumb._LinesStarts.Remove(this);

                        var valueBorderVM = value.MyEntityViewModel as IChartEntityViewModel;
                        if (valueBorderVM != null && endBorderVM != null)
                            valueBorderVM.NewChildAddedFromChart(endBorderVM);

                        _StartThumb._LinesStarts.Remove(this);
                    }

                    _StartThumb = value;
                    _StartThumb._LinesStarts.Add(this);
                    if (!_LoadingFile && Segments != null && Segments.Count > 0)
                    {
                        var segment = Segments[0];
                        segment.ChangeStartSource(value);
                    }
                }
            }
        }
        public EntityConnectingThumb EndThumb
        {
            get { return _EndThumb; }
            set
            {
                if (value == null)
                {
                    if (!_LoadingFile && _EndThumb != null)
                    {
                        var startBorderVM = _StartThumb != null ? _StartThumb.MyEntityViewModel as IChartEntityViewModel : null;
                        var endBorderVM = _EndThumb.MyEntityViewModel as IChartEntityViewModel;
                        if (startBorderVM != null && endBorderVM != null)
                            endBorderVM.RemoveMeAsAChild();
                        _EndThumb._LinesEnds.Remove(this);
                        _EndThumb = null;
                    }
                }
                else if (!value.Equals(_EndThumb))
                {
                    var startBorderVM = _StartThumb.MyEntityViewModel as IChartEntityViewModel;
                    IChartEntityViewModel endBorderVM;
                    if (!_LoadingFile && _EndThumb != null)
                    {
                        endBorderVM = _EndThumb.MyEntityViewModel as IChartEntityViewModel;
                    
                        if (startBorderVM != null && endBorderVM != null)
                            endBorderVM.RemoveMeAsAChild();
                        _EndThumb._LinesEnds.Remove(this);
                    }

                    _EndThumb = value;
                    endBorderVM = _EndThumb.MyEntityViewModel as IChartEntityViewModel;
                    if (!_LoadingFile)
                    {
                        var valueBorderVM = value.MyEntityViewModel as IChartEntityViewModel;
                        if (valueBorderVM != null && startBorderVM != null)
                            startBorderVM.NewChildAddedFromChart(valueBorderVM);
                    }
                    _EndThumb._LinesEnds.Add(this);
                    if (!_LoadingFile && Segments != null && Segments.Count > 0)
                    {
                        var endSegment = Segments.Last() as ILineSegmentControlEndPoint;
                        endSegment.ChangeEndSource(value);
                    }
                }
            }
        }
        #endregion

        #region helpers
        private void SetLineSelectedBrush()
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
                }
            }
        }
        internal void DoAllSegments(Action<aLineSegmentBase> action)
        {
            foreach (var item in Segments)
            {
                action(item);
            }
        }
        #endregion

        #region mouse
        //protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    if ((bool)e.NewValue)
        //        ShowAllThumbs();
        //    else
        //        HideAllThumbs();
        //}
        private void HitboxOnIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var connection = (LineConnection)((Polyline)sender).Tag;
            if ((bool)e.NewValue)
            {
                ChartCustomControl.Instance._MouseEnterOnVisual++;
                connection.ShowAllThumbs();
            }
            else
            {
                ChartCustomControl.Instance._MouseEnterOnVisual--;
                connection.HideAllThumbs();
            }
        }
        protected void HitBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(null);
            e.Handled = true;
        }
        protected void HitBoxPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(null);
            e.Handled = true;
        }
        #endregion

        #region polyline
        public ObservableCollection<aLineSegmentBase> Segments
        {
            get { return (ObservableCollection<aLineSegmentBase>)GetValue(SegmentsProperty); }
            set { SetValue(SegmentsProperty, value); }
        }
        public static readonly DependencyProperty SegmentsProperty =
            DependencyProperty.Register("Segments", typeof(ObservableCollection<aLineSegmentBase>), typeof(LineConnection), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsMeasure, SegmentsPropertyChanged));
        private static void SegmentsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LineConnection)d).SegmentsCollectionChanged((INotifyCollectionChanged)e.OldValue, (INotifyCollectionChanged)e.NewValue);
        }
        private void SegmentsCollectionChanged(INotifyCollectionChanged oldValue, INotifyCollectionChanged newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= SegmentsChanged;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += SegmentsChanged;
            }
        }
        private void SegmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var coll = e.OldItems.Cast<aLineSegmentBase>();

                foreach (var segment in coll)
                {
                    segment.RemoveAllThumbsFromCanvas();
                    //var canvasColl = ChartCustomControl.Instance.ChartCanvas.Children;
                    //if (canvasColl.Contains(segment))
                    //    canvasColl.Remove(segment);
                }
            }
        }

        public new void InvalidateMeasure()
        {
            base.InvalidateMeasure();
            _HitboxPolyLine.InvalidateMeasure();
        }
        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowPolyline.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                // Clear out the PathGeometry.
                _Pathgeo.Figures.Clear();
                _PathfigLine.Segments.Clear();
                _HitboxPolyLine.Points.Clear();

                // Try to avoid unnecessary indexing exceptions.
                if (Segments.Count > 0)
                {
                    // Define a PathFigure containing the points.
                    _PathfigLine.StartPoint = Segments[0].Start;
                    _PolysegLine.Points.Clear();

                    for (int i = 0; i < Segments.Count; i++)
                    {
                        var current = Segments[i];
                        switch (current._Type)
                        {
                            case LineSegmentTypesEnum.Start:
                            case LineSegmentTypesEnum.Normal:
                                _PolysegLine.Points.Add(current.Start);
                                _HitboxPolyLine.Points.Add(current.Start);
                                break;
                            case LineSegmentTypesEnum.Unique:
                            case LineSegmentTypesEnum.End:
                                _PolysegLine.Points.Add(current.Start);
                                _PolysegLine.Points.Add(current.End);
                                _HitboxPolyLine.Points.Add(current.Start);
                                _HitboxPolyLine.Points.Add(current.End);
                                break;
                        }
                    }

                    _PathfigLine.Segments.Add(_PolysegLine);
                    _Pathgeo.Figures.Add(_PathfigLine);
                }

                // Call the base property to add arrows on the ends.
                return base.DefiningGeometry;
            }
        }
        #endregion

        #region have thumbs
        private bool _ThumbsHidden;
        private void HideAllThumbsExceptLineDraggers()
        {
            if (ChartCustomControl.Instance._InternalShowAllHiddableThumbs)
                return;
            foreach (var item in GetAllThumbs())
            {
                if (item is LineDragger)
                {
                    if (item.Visibility != Visibility.Visible)
                        item.Visibility = Visibility.Visible;
                    continue;
                }
                if (item.Visibility != Visibility.Hidden)
                    item.Visibility = Visibility.Hidden;
            }
            _ThumbsHidden = false;
        }
        public void HideAllThumbs()
        {
            if (ChartCustomControl.Instance._InternalShowAllHiddableThumbs || _ThumbsHidden)
                return;

            foreach (var segment in Segments)
            {
                segment.HideAllThumbs();
            }
            _ThumbsHidden = true;
        }
        public void ShowAllThumbs()
        {
            if (ChartCustomControl.Instance._InternalShowAllHiddableThumbs || !_ThumbsHidden)
                return;

            //var canvas = ChartCustomControl.Instance;
            //var middleCanvas = this.TranslatePoint(_MiddlePoint, canvas.ChartCanvas);
            //var sizeM = LineDivider.Size * 0.5d;
            //canvas.MoveElementToCoordinates(_LineDivider, middleCanvas.X - sizeM, middleCanvas.Y - sizeM);

            //_LineMathFunction.Change2Points(Source, Destination);
            //var margin = Properties.Settings.Default.LinesConnectersYMargin;//_LineMathFunction.Slope > 0 ? Properties.Settings.Default.LinesConnectersYMargin : -1 * Properties.Settings.Default.LinesConnectersYMargin;
            //var p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Source, margin);
            //canvas.MoveElementToCoordinates(
            //    _LineConnecters[0],
            //    p.X - (LineConnecter.Size * 0.5d),
            //    p.Y - (LineConnecter.Size * 0.5d));
            //p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Destination, -margin);
            //canvas.MoveElementToCoordinates(
            //    _LineConnecters[1],
            //    p.X - (LineConnecter.Size * 0.5d),
            //    p.Y - (LineConnecter.Size * 0.5d));

            foreach (var segment in Segments)
            {
                segment.ShowAllThumbs();
            }
            _ThumbsHidden = false;
        }
        public IEnumerable<IChartThumb> GetAllThumbs()
        {
            foreach (var segment in Segments)
            {
                switch (segment._Type)
                {
                    case LineSegmentTypesEnum.Unique:
                        var unique = (LineUniqueSegment)segment;
                        yield return unique._StartLineConnecter;
                        yield return unique._LineDivider;
                        yield return unique._EndLineConnecter;
                        break;
                    case LineSegmentTypesEnum.Start:
                        var start = (LineStartSegment)segment;
                        yield return start._LineConnecter;
                        yield return start._LineDivider;
                        break;
                    case LineSegmentTypesEnum.Normal:
                        var normal = (LineNormalSegment)segment;
                        yield return normal.MyLineDragger;
                        yield return normal._LineDivider;
                        break;
                    case LineSegmentTypesEnum.End:
                        var end = (LineEndSegment)segment;
                        yield return end._LineDivider;
                        yield return end._LineConnecter;
                        break;
                }
            }
            yield break;
            //if (_LineDraggers != null)
            //{
            //    if (_LineDraggers[0] != null)
            //        yield return _LineDraggers[0];
            //    if (_LineDraggers[1] != null)
            //        yield return _LineDraggers[1];
            //}
            //if (_LineConnecters != null)
            //{
            //    if (_LineConnecters[0] != null)
            //        yield return _LineConnecters[0];
            //    if (_LineConnecters[1] != null)
            //        yield return _LineConnecters[1];
            //}
            //yield return _LineDivider;
            //yield break;
        }
        public void DoActionAllThumbs(Action<IChartThumb> action)
        {
            foreach (var item in GetAllThumbs())
            {
                action(item);
            }
        }
        #endregion

        #region segments
        public void DivideLine(aLineSegmentBase segment)
        {
            int index = Segments.IndexOf(segment);
            if (index == -1)
                throw new ArgumentOutOfRangeException();

            var redoParams = new object[] { this, segment, index };
            var undoParList = new List<object>() { this, segment, index };

            switch (segment._Type)
            {
                case LineSegmentTypesEnum.Unique:
                    //Segments.Remove(segment);
                    var pp = new PropertyPath(EntityConnectingThumb.AnchorPointProperty);
                    var sb = new Binding() { Source = StartThumb, Path = pp };
                    var eb = new Binding() { Source = EndThumb, Path = pp };
                    var endSeg = new LineEndSegment(this, segment.MiddlePoint, eb, 1);
                    undoParList.Add(endSeg);
                    var startSeg = new LineStartSegment(this, endSeg.MyLineDragger, sb, segment.MiddlePoint);
                    undoParList.Add(startSeg);
                    var segments = new ObservableCollection<aLineSegmentBase>();
                    segments.Add(startSeg);
                    segments.Add(endSeg);
                    Segments.RemoveAt(0);
                    Segments = segments;
                    break;
                case LineSegmentTypesEnum.Start:
                    var nextSeg = (ILineSegmentWithDragger)Segments[1];
                    var newSeg = new LineNormalSegment(
                        this,
                        Segments[0].MiddlePoint,
                        Segments[0].End,
                        ((ILineSegmentWithDragger)Segments[1]).MyLineDragger,
                        1);
                    undoParList.Add(newSeg);

                    var dragRef = (ILineSegmentWithNextSegmentDraggerReference)segment;
                    undoParList.Add(dragRef.NextSegmentDragger);
                    dragRef.NextSegmentDragger = newSeg.MyLineDragger;

                    Segments.Insert(1, newSeg);
                    break;
                case LineSegmentTypesEnum.Normal:
                    nextSeg = (ILineSegmentWithDragger)Segments[index + 1];
                    newSeg = new LineNormalSegment(
                        this,
                        segment.MiddlePoint,
                        segment.End,
                        nextSeg.MyLineDragger,
                        index);
                    undoParList.Add(newSeg);

                    var previous = (ILineSegmentWithNextSegmentDraggerReference)Segments[index - 1];
                    undoParList.Add(previous.NextSegmentDragger);
                    previous.NextSegmentDragger = newSeg.MyLineDragger;

                    Segments.Insert(index, newSeg);
                    break;
                case LineSegmentTypesEnum.End:
                    var current = (LineEndSegment)segment;
                    newSeg = new LineNormalSegment(
                        this,
                        segment.Start,
                        segment.MiddlePoint,
                        current.MyLineDragger,
                        index);
                    undoParList.Add(newSeg);

                    undoParList.Add(new Point(current.Start.X, current.Start.Y));
                    ChartCustomControl.Instance.MoveElementToCoordinates(current.MyLineDragger, current.MiddlePoint.X, current.MiddlePoint.Y);

                    previous = (ILineSegmentWithNextSegmentDraggerReference)Segments[index - 1];
                    undoParList.Add(previous.NextSegmentDragger);
                    previous.NextSegmentDragger = newSeg.MyLineDragger;

                    Segments.Insert(index, newSeg);
                    break;
            }

            ReSetSegmentIndexes();
            UndoRedoCommandManager.Instance.NewCommand(
                Properties.UndoRedoNames.Default.LineConnection_DivideSegment, DivideLineUndoAction, undoParList.ToArray(), DivideLineRedoAction, redoParams);
        }
        public void RemoveSegment(LineDragger dragger)
        {
            var seg1 = dragger._Segment;
            var index1 = Segments.IndexOf(seg1);
            if (index1 < 1)
                throw new ArgumentOutOfRangeException();

            var seg0 = Segments[index1 - 1];

            var redoParams = new List<object>() { this, seg0, seg1, index1 };
            var undoParams = new List<object>() { this, seg0, seg1, index1 };

            LineDragger dragger1, dragger2;
            Point midPoint;

            switch (seg0._Type)
            {
                //case LineSegmentTypesEnum.Unique: => impossible, no line dragger
                case LineSegmentTypesEnum.Start:
                    if (seg1._Type == LineSegmentTypesEnum.End)
                    {
                        var newSeg = new LineUniqueSegment(
                            this,
                            StartThumb,
                            EndThumb);
                        redoParams.Add(newSeg);

                        seg0.RemoveAllThumbsFromCanvas();
                        seg1.RemoveAllThumbsFromCanvas();

                        var newColl = new ObservableCollection<aLineSegmentBase>() { newSeg };
                        Segments = newColl;
                    }
                    else //normal segment
                    {
                        dragger2 = ((ILineSegmentWithDragger)Segments[index1 + 1]).MyLineDragger;
                        dragger1 = ((ILineSegmentWithDragger)seg1).MyLineDragger;
                        midPoint = new Point(seg1.MiddlePoint.X, seg1.MiddlePoint.Y);

                        redoParams.Add(midPoint);
                        undoParams.Add(((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger);
                        undoParams.Add(new Point(dragger2.AnchorPoint.X, dragger2.AnchorPoint.Y));

                        Segments.Remove(seg1);

                        ((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger = dragger2;
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger2, midPoint.X, midPoint.Y);
                    }
                    break;
                case LineSegmentTypesEnum.Normal:
                    if (seg1._Type == LineSegmentTypesEnum.Normal)
                    {
                        dragger2 = ((ILineSegmentWithDragger)Segments[index1 + 1]).MyLineDragger;
                        dragger1 = ((ILineSegmentWithDragger)seg1).MyLineDragger;
                        midPoint = new Point(seg1.MiddlePoint.X, seg1.MiddlePoint.Y);

                        redoParams.Add(midPoint);
                        undoParams.Add(((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger);
                        undoParams.Add(new Point(dragger2.AnchorPoint.X, dragger2.AnchorPoint.Y));

                        Segments.Remove(seg1);

                        ((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger = dragger2;
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger2, midPoint.X, midPoint.Y);
                    }
                    else //seg1 is end
                    {
                        var dragger0 = ((ILineSegmentWithDragger)seg0).MyLineDragger;
                        dragger1 = ((ILineSegmentWithDragger)seg1).MyLineDragger;
                        undoParams.Add(new Point(dragger1.AnchorPoint.X, dragger1.AnchorPoint.Y));
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger1, dragger0.AnchorPoint.X, dragger0.AnchorPoint.Y);
                        redoParams.Add(new Point(dragger0.AnchorPoint.X, dragger0.AnchorPoint.Y));

                        var previous = (ILineSegmentWithNextSegmentDraggerReference)Segments[index1 - 1];
                        redoParams.Add(previous);
                        undoParams.Add(previous);
                        previous.NextSegmentDragger = dragger1;

                        Segments.Remove(seg0);
                    }
                    break;
            }

            ReSetSegmentIndexes();
            UndoRedoCommandManager.Instance.NewCommand(
                Properties.UndoRedoNames.Default.LineConnection_RemoveSegment, RemoveSegmentUndoAction, undoParams.ToArray(), RemoveSegmentRedoAction, redoParams.ToArray());
        }
        private void ReSetSegmentIndexes()
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                var segment = Segments[i];
                segment._Index = i;
            }
        }
        #endregion

        #region undo/redo segments
        private void DivideLineRedoAction(object[] parameters)
        {
            var connection = parameters[0] as LineConnection;
            var connSegments = connection.Segments;
            var dividedSegment = parameters[1] as aLineSegmentBase;
            var index = (int)parameters[2];

            switch (dividedSegment._Type)
            {
                case LineSegmentTypesEnum.Unique:
                    //connSegments.Remove(segment);
                    var pp = new PropertyPath(EntityConnectingThumb.AnchorPointProperty);
                    var sb = new Binding() { Source = connection.StartThumb, Path = pp };
                    var eb = new Binding() { Source = connection.EndThumb, Path = pp };
                    var endSeg = new LineEndSegment(this, dividedSegment.MiddlePoint, eb, 1);
                    var startSeg = new LineStartSegment(this, endSeg.MyLineDragger, sb, dividedSegment.MiddlePoint);
                    connSegments.Remove(dividedSegment);
                    connSegments.Add(startSeg);
                    connSegments.Add(endSeg);
                    break;
                case LineSegmentTypesEnum.Start:
                    var nextSeg = (ILineSegmentWithDragger)connSegments[1];
                    var newSeg = new LineNormalSegment(
                        this,
                        connSegments[0].MiddlePoint,
                        connSegments[0].End,
                        ((ILineSegmentWithDragger)connSegments[1]).MyLineDragger,
                        0);
                    ((ILineSegmentWithNextSegmentDraggerReference)dividedSegment).NextSegmentDragger = newSeg.MyLineDragger;
                    connSegments.Insert(1, newSeg);
                    break;
                case LineSegmentTypesEnum.Normal:
                    nextSeg = (ILineSegmentWithDragger)connSegments[index + 1];
                    newSeg = new LineNormalSegment(
                        this,
                        dividedSegment.MiddlePoint,
                        dividedSegment.End,
                        nextSeg.MyLineDragger,
                        0);
                    ((ILineSegmentWithNextSegmentDraggerReference)connSegments[index - 1]).NextSegmentDragger = newSeg.MyLineDragger;
                    connSegments.Insert(index, newSeg);
                    break;
                case LineSegmentTypesEnum.End:
                    var current = (LineEndSegment)dividedSegment;
                    newSeg = new LineNormalSegment(
                        this,
                        dividedSegment.Start,
                        dividedSegment.MiddlePoint,
                        current.MyLineDragger,
                        0);
                    ChartCustomControl.Instance.MoveElementToCoordinates(current.MyLineDragger, current.MiddlePoint.X, current.MiddlePoint.Y);
                    ((ILineSegmentWithNextSegmentDraggerReference)connSegments[index - 1]).NextSegmentDragger = newSeg.MyLineDragger;
                    connSegments.Insert(index, newSeg);
                    break;
            }
            ReSetSegmentIndexes();
        }
        private void DivideLineUndoAction(object[] parameters)
        {
            var connection = parameters[0] as LineConnection;
            var connSegments = connection.Segments;
            var dividedSegment = parameters[1] as aLineSegmentBase;
            var dindex = (int)parameters[2];

            switch (dividedSegment._Type)
            {
                case LineSegmentTypesEnum.Unique:
                    var start = (LineStartSegment)parameters[4];
                    var end = (LineEndSegment)parameters[3];
                    connSegments.Remove(start);
                    connSegments.Remove(end);
                    connSegments.Add(dividedSegment);
                    dividedSegment.AddAllThumbsToCanvas();
                    break;
                case LineSegmentTypesEnum.Start:
                    var normal = (LineNormalSegment)parameters[3];
                    connSegments.Remove(normal);
                    ((ILineSegmentWithNextSegmentDraggerReference)dividedSegment).NextSegmentDragger = (LineDragger)parameters[4];
                    break;
                case LineSegmentTypesEnum.Normal:
                    normal = (LineNormalSegment)parameters[3];
                    var previous = (ILineSegmentWithNextSegmentDraggerReference)connSegments[dindex - 1];
                    connSegments.Remove(normal);
                    previous.NextSegmentDragger = (LineDragger)parameters[4];
                    break;
                case LineSegmentTypesEnum.End:
                    normal = (LineNormalSegment)parameters[3];
                    connSegments.Remove(normal);
                    var point = (Point)parameters[4];
                    end = (LineEndSegment)dividedSegment;
                    ChartCustomControl.Instance.MoveElementToCoordinates(end.MyLineDragger, point.X, point.Y);
                    previous = (ILineSegmentWithNextSegmentDraggerReference)connSegments[dindex - 1];
                    previous.NextSegmentDragger = (LineDragger)parameters[5];
                    break;
            }
            ReSetSegmentIndexes();
        }
        private void RemoveSegmentRedoAction(object[] parameters)
        {
            var connection = parameters[0] as LineConnection;
            var seg0 = parameters[1] as aLineSegmentBase;
            var seg1 = parameters[2] as aLineSegmentBase;
            var index1 = (int)parameters[3];

            LineDragger dragger2;
            Point midPoint;

            switch (seg0._Type)
            {
                case LineSegmentTypesEnum.Start:
                    if (seg1._Type == LineSegmentTypesEnum.End)
                    {
                        var newSeg = parameters[4] as aLineSegmentBase;

                        seg0.RemoveAllThumbsFromCanvas();
                        seg1.RemoveAllThumbsFromCanvas();

                        var newColl = new ObservableCollection<aLineSegmentBase>() { newSeg };
                        connection.Segments = newColl;
                    }
                    else
                    {
                        dragger2 = ((ILineSegmentWithDragger)connection.Segments[index1 + 1]).MyLineDragger;
                        midPoint = (Point)parameters[4];

                        connection.Segments.Remove(seg1);

                        ((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger = dragger2;
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger2, midPoint.X, midPoint.Y);
                    }
                    break;
                case LineSegmentTypesEnum.Normal:
                    if (seg1._Type == LineSegmentTypesEnum.Normal)
                    {
                        dragger2 = ((ILineSegmentWithDragger)connection.Segments[index1 + 1]).MyLineDragger;
                        midPoint = (Point)parameters[4];

                        connection.Segments.Remove(seg1);

                        ((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger = dragger2;
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger2, midPoint.X, midPoint.Y);
                    }
                    else
                    {
                        var dragger0 = ((ILineSegmentWithDragger)seg0).MyLineDragger;
                        var dragger1 = ((ILineSegmentWithDragger)seg1).MyLineDragger;
                        var p = (Point)parameters[4];
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger1, p.X, p.Y);

                        connection.Segments.Remove(seg0);

                        var previous = (ILineSegmentWithNextSegmentDraggerReference)parameters[5];
                        previous.NextSegmentDragger = dragger1;
                    }
                    break;
            }
            ReSetSegmentIndexes();
        }
        private void RemoveSegmentUndoAction(object[] parameters)
        {
            var connection = parameters[0] as LineConnection;
            var seg0 = parameters[1] as aLineSegmentBase;
            var seg1 = parameters[2] as aLineSegmentBase;
            var index1 = (int)parameters[3];

            LineDragger dragger2;
            Point p;

            switch (seg0._Type)
            {
                case LineSegmentTypesEnum.Start:
                    if (seg1._Type == LineSegmentTypesEnum.End)
                    {
                        seg0.AddAllThumbsToCanvas();
                        seg1.AddAllThumbsToCanvas();

                        var newColl = new ObservableCollection<aLineSegmentBase>() { seg0, seg1 };
                        connection.Segments = newColl;
                    }
                    else //normal segment
                    {
                        dragger2 = ((ILineSegmentWithDragger)connection.Segments[index1 + 1]).MyLineDragger;

                        connection.Segments.Add(seg1);
                        seg1.AddAllThumbsToCanvas();

                        ((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger = (LineDragger)parameters[4];
                        p = (Point)parameters[5];
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger2, p.X, p.Y);
                    }
                    break;
                case LineSegmentTypesEnum.Normal:
                    if (seg1._Type == LineSegmentTypesEnum.Normal)
                    {
                        dragger2 = ((ILineSegmentWithDragger)connection.Segments[index1 + 1]).MyLineDragger;

                        connection.Segments.Add(seg1);
                        seg1.AddAllThumbsToCanvas();

                        ((ILineSegmentWithNextSegmentDraggerReference)seg0).NextSegmentDragger = (LineDragger)parameters[4];
                        p = (Point)parameters[5];
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger2, p.X, p.Y);
                    }
                    else
                    {
                        var dragger0 = ((ILineSegmentWithDragger)seg0).MyLineDragger;
                        var dragger1 = ((ILineSegmentWithDragger)seg1).MyLineDragger;
                        p = (Point)parameters[4];
                        ChartCustomControl.Instance.MoveElementToCoordinates(dragger1, p.X, p.Y);

                        connection.Segments.Add(seg0);
                        seg0.AddAllThumbsToCanvas();

                        var previous = (ILineSegmentWithNextSegmentDraggerReference)parameters[5];
                        previous.NextSegmentDragger = dragger0;
                    }
                    break;
            }
            ReSetSegmentIndexes();
        }
        #endregion

        #region remove
        public bool AlreadyRemovedByParent { get; set; }

        public void RemoveThis()
        {
            if (AlreadyRemovedByParent)
                return;

            var undoParams = new object[3] { this, StartThumb, EndThumb };
            var redoParams = new object[1] { this };
            if (!UndoRedoCommandManager.Instance.LastCommandIsNull && UndoRedoCommandManager.Instance.LastCommandName.IndexOf("remove", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                UndoRedoCommandManager.Instance.AddToLastCommand(x => RemoveConnectionUndoAction(undoParams), undoParams, x => RemoveConnectionRedoAction(redoParams), redoParams);
            }
            else
            {
                UndoRedoCommandManager.Instance.NewCommand(
                    Properties.UndoRedoNames.Default.LineConnection_Remove, RemoveConnectionUndoAction, undoParams, RemoveConnectionRedoAction, redoParams);
            }

            if (IsSelected)
                ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemDeselected(this);

            foreach (var item in Segments)
            {
                item.RemoveAllThumbsFromCanvas();
            }
            StartThumb = null;
            EndThumb = null;
            ChartCustomControl.Instance.ChartCanvas.Children.Remove(this);
        }
        public void AddToLastCommandMyUndoRedoCommands()
        {
            if (UndoRedoCommandManager.Instance.LastCommandIsNull)
                return;

            var undoParams = new object[3] { this, StartThumb, EndThumb };
            var redoParams = new object[1] { this };
            UndoRedoCommandManager.Instance.AddToLastCommand(x => RemoveConnectionUndoAction(x), undoParams, x => RemoveConnectionRedoAction(x), redoParams);
        }
        public void RemovedByParent()
        {
            AlreadyRemovedByParent = true;
            if (IsSelected)
                ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemDeselected(this);

            foreach (var item in Segments)
            {
                item.RemoveAllThumbsFromCanvas();
            }
            StartThumb = null;
            EndThumb = null;
            ChartCustomControl.Instance.ChartCanvas.Children.Remove(this);
        }
        private void RemoveConnectionRedoAction(object[] parameters)
        {
            var connection = (LineConnection)parameters[0];
            foreach (var item in connection.Segments)
            {
                item.RemoveAllThumbsFromCanvas();
            }
            StartThumb = null;
            EndThumb = null;
            ChartCustomControl.Instance.ChartCanvas.Children.Remove(connection);
        }
        private void RemoveConnectionUndoAction(object[] parameters)
        {
            AlreadyRemovedByParent = false;
            var connection = (LineConnection)parameters[0];
            ChartCustomControl.Instance.ChartCanvas.Children.Add(connection);
            connection.StartThumb = (EntityConnectingThumb)parameters[1];
            connection.EndThumb = (EntityConnectingThumb)parameters[2];
            foreach (var item in connection.Segments)
            {
                item.AddAllThumbsToCanvas();
            }
        }
        #endregion

        protected override void UpdateSelectedVisualEffect()
        {
            if (IsSelected)
                Stroke = _SelectedBrush;
            else
                Stroke = Brushes.Black;
        }

        public LineConnectionSaveProxy GetSerializationProxy()
        {
            return new LineConnectionSaveProxy()
            {
                StartThumb = StartThumb.Type,
                EndThumb = EndThumb.Type,
                StartVM = StartThumb.MyEntityViewModel.ViewModelId,
                EndVM = EndThumb.MyEntityViewModel.ViewModelId,
                Segments = Segments
                    .Select(x =>
                    {
                        switch (x._Type)
                        {
                            case LineSegmentTypesEnum.Unique:
                                return (aLineSegmentProxy)((LineUniqueSegment)x).GetSerializationProxy();
                            case LineSegmentTypesEnum.Start:
                                return (aLineSegmentProxy)((LineStartSegment)x).GetSerializationProxy();
                            case LineSegmentTypesEnum.Normal:
                                return (aLineSegmentProxy)((LineNormalSegment)x).GetSerializationProxy();
                            default://case LineSegmentTypesEnum.End:
                                return (aLineSegmentProxy)((LineEndSegment)x).GetSerializationProxy();
                        }
                    })
                    .ToList()
            };
        }
    }
}
