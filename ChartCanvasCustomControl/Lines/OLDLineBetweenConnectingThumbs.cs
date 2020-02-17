using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Thumbs;
using Math471;
using Math471.StraightLine;
using Petzold.Media2D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChartCanvasNamespace.Lines
{
    //public class LineBetweenConnectingThumbs : Selection.ChartEntityUserControlCanBeSelected, IChartObjectCanBeRemoved, IChartHaveHiddableThumbs, INotifyPropertyChanged
    //{
    //    #region constructors/init
    //    public LineBetweenConnectingThumbs(EntityConnectingThumb start, EntityConnectingThumb end)
    //    {
    //        SetLineSelectedBrush();

    //        //MyConnection = new LineConnection(start, end) { FirstLine = this, LastLine = this };
    //    }
    //    public LineBetweenConnectingThumbs(LineConnection group)
    //    {
    //        SetLineSelectedBrush();

    //        MyConnection = group;
    //        //MyConnection.LastLine = this;
    //    }
    //    private void SetLineSelectedBrush()
    //    {
    //        if (_SelectedBrush == null)
    //        {
    //            lock (_LockObject)
    //            {
    //                if (_SelectedBrush == null)
    //                {
    //                    var resourceDict = new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/ChartCanvas;component/Resources/ResDict0.xaml") };

    //                    _SelectedBrush = (SolidColorBrush)resourceDict["SelectionLineSelectedBrush"];
    //                }
    //            }
    //        }
    //    }
    //    #endregion

    //    #region fields
    //    private static object _LockObject = new object();
    //    private static SolidColorBrush _SelectedBrush = null;
    //    private ArrowLine _ArrowLine;
    //    private TwoPointsStraightLineEquation _LineMathFunction;
    //    internal LineDragger[] _LineDraggers;
    //    private LineDivider _LineDivider;
    //    private LineConnecter[] _LineConnecters;
    //    private Point _MiddlePoint;
    //    #endregion

    //    #region properties
    //    public LineConnection MyConnection { get; private set; }
    //    public LineBetweenConnectingThumbs PreviousLine { get; internal set; }
    //    public LineBetweenConnectingThumbs NextLine { get; internal set; }
    //    #endregion

    //    #region dependency properties
    //    public Point Source { get { return (Point)this.GetValue(SourceProperty); } set { this.SetValue(SourceProperty, value); } }
    //    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Point), typeof(LineBetweenConnectingThumbs), 
    //        new FrameworkPropertyMetadata(default(Point)));

    //    public Point Destination { get { return (Point)this.GetValue(DestinationProperty); } set { this.SetValue(DestinationProperty, value); } }
    //    public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register("Destination", typeof(Point), typeof(LineBetweenConnectingThumbs), 
    //        new FrameworkPropertyMetadata(default(Point)));
    //    #endregion

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void OnPropertyChanged(string prop)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    //        }
    //    }

    //    #region mouse
    //    private bool IsMouseOverAnyThumb()
    //    {
    //        if (_LineDivider.IsMouseOver)
    //            return true;

    //        if (_LineDraggers != null)
    //        {
    //            if (_LineDraggers[0] != null && _LineDraggers[0].IsMouseOver)
    //                return true;
    //            if (_LineDraggers[1] != null && _LineDraggers[1].IsMouseOver)
    //                return true;
    //        }

    //        if(_LineConnecters != null)
    //        {
    //            if (_LineConnecters[0] != null && _LineConnecters[0].IsMouseOver)
    //                return true;
    //            if (_LineConnecters[1] != null && _LineConnecters[1].IsMouseOver)
    //                return true;
    //        }

    //        return false;

    //        //return _LineDivider.IsMouseOver || 
    //        //    (_LineDraggers != null &&
    //        //    (_LineDraggers[0] != null && _LineDraggers[0].IsMouseOver) ||
    //        //    (_LineDraggers[1] != null && _LineDraggers[1].IsMouseOver));
    //    }
    //    protected override void OnMouseEnter(MouseEventArgs e)
    //    {
    //        if (IsMouseOverAnyThumb())
    //            return;

    //        ShowAllThumbs();
    //        e.Handled = true;
    //    }
    //    protected override void OnMouseLeave(MouseEventArgs e)
    //    {
    //        if (IsMouseOverAnyThumb())
    //            return;

    //        HideAllThumbs();
    //        e.Handled = true;
    //    }
    //    #endregion

    //    #region paths / lines
    //    private class SourceDestBindings
    //    {
    //        public Binding SourceX;
    //        public Binding SourceY;
    //        public Binding DestinationX;
    //        public Binding DestinationY;
    //    }
    //    private SourceDestBindings GetSourceDestBindings(BindingMode mode = BindingMode.TwoWay, IValueConverter converter = null, object converterParameter = null)
    //    {
    //        return new SourceDestBindings()
    //        {
    //            SourceX = new Binding("Source.X") { Source = this, Mode = mode, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Converter = converter, ConverterParameter = converterParameter},
    //            SourceY = new Binding("Source.Y") { Source = this, Mode = mode, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Converter = converter, ConverterParameter = converterParameter },
    //            DestinationX = new Binding("Destination.X") { Source = this, Mode = mode, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Converter = converter, ConverterParameter = converterParameter },
    //            DestinationY = new Binding("Destination.Y") { Source = this, Mode = mode, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Converter = converter, ConverterParameter = converterParameter }
    //        };
    //    }
    //    private void DrawPaths()
    //    {
    //        var grid = Content as Grid;
    //        if (grid != null)
    //        {
    //            grid.Children.Clear();
    //        }
    //        else
    //        {
    //            grid = new Grid();
    //            Content = grid;
    //        }

    //        _ArrowLine = new ArrowLine();
    //        var bindings = GetSourceDestBindings();
    //        BindingOperations.SetBinding(_ArrowLine, ArrowLine.X1Property, bindings.SourceX);
    //        BindingOperations.SetBinding(_ArrowLine, ArrowLine.Y1Property, bindings.SourceY);
    //        BindingOperations.SetBinding(_ArrowLine, ArrowLine.X2Property, bindings.DestinationX);
    //        BindingOperations.SetBinding(_ArrowLine, ArrowLine.Y2Property, bindings.DestinationY);

    //        _ArrowLine.Stroke = Brushes.Black;
    //        _ArrowLine.StrokeThickness = 1;
    //        _ArrowLine.IsHitTestVisible = false;
    //        _ArrowLine.Name = "path";
    //        _ArrowLine.MinWidth = 1;
    //        _ArrowLine.MinHeight = 1;
    //        _ArrowLine.ArrowEnds = ArrowEnds.End;
    //        _ArrowLine.ArrowLength = 5;
    //        _ArrowLine.IsArrowClosed = false;

    //        var segment = new aLineSegment(default(Point), true);
    //        var figure = new PathFigure(default(Point), new[] { segment }, false);
    //        var geometry = new PathGeometry(new[] { figure });
    //        var sourceBinding = new Binding { Source = this, Path = new PropertyPath(SourceProperty) };
    //        var destinationBinding = new Binding { Source = this, Path = new PropertyPath(DestinationProperty) };
    //        BindingOperations.SetBinding(figure, PathFigure.StartPointProperty, sourceBinding);
    //        BindingOperations.SetBinding(segment, aLineSegment.PointProperty, destinationBinding);
    //        //_LinePath = new Path
    //        //{
    //        //    Data = geometry,
    //        //    StrokeThickness = 1,
    //        //    Stroke = Brushes.Black,
    //        //    MinWidth = 1,
    //        //    MinHeight = 1,
    //        //    IsHitTestVisible = false,
    //        //    Name = "path"
    //        //};
    //        var hitbox = new Path
    //        {
    //            Data = geometry,
    //            StrokeThickness = 10,
    //            Stroke = Brushes.Transparent,
    //            Fill = Brushes.Transparent,
    //            MinWidth = 1,
    //            MinHeight = 1,
    //            IsHitTestVisible = true,
    //            Name = "hitbox"
    //        };
    //        Panel.SetZIndex(_ArrowLine, Properties.Settings.Default.ZIndex_LinePath);
    //        Panel.SetZIndex(hitbox, Properties.Settings.Default.ZIndex_LineHitBox);
    //        grid.Children.Add(_ArrowLine);
    //        grid.Children.Add(hitbox);
    //        Panel.SetZIndex(grid, Properties.Settings.Default.ZIndex_LineHitBox);
    //    }
    //    public void Draw()
    //    {
    //        DrawPaths();

    //        _MiddlePoint = GetMiddlePoint();
    //        _LineDivider = new LineDivider(this);

    //        var canvas = ChartCustomControl.Instance;
    //        var sizeM = LineDivider.Size * 0.5d;
    //        canvas.AddElementInCoordinates(_LineDivider, _MiddlePoint.X - sizeM, _MiddlePoint.Y - sizeM);

    //        _LineMathFunction = new TwoPointsStraightLineEquation(Source, Destination);
    //        _LineConnecters = new LineConnecter[2] { new LineConnecter(this), new LineConnecter(this) };
    //        var margin = Properties.Settings.Default.LinesConnectersYMargin;// _LineMathFunction.Slope > 0 ? Properties.Settings.Default.LinesConnectersYMargin : -1 * Properties.Settings.Default.LinesConnectersYMargin;
    //        var p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Source, margin);
    //        canvas.AddElementInCoordinates(
    //            _LineConnecters[0],
    //            p.X - (LineConnecter.Size * 0.5d),
    //            p.Y - (LineConnecter.Size * 0.5d));
    //        p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Destination, -margin);
    //        canvas.AddElementInCoordinates(
    //            _LineConnecters[1],
    //            p.X - (LineConnecter.Size * 0.5d),
    //            p.Y - (LineConnecter.Size * 0.5d));

    //        //canvas.AddElementInCoordinates(_LineConnecters[0], Source.X - (LineConnecter.Size * 0.5d), Source.Y - (LineConnecter.Size * 0.5d));
    //        //canvas.AddElementInCoordinates(_LineConnecters[1], Destination.X - (LineConnecter.Size * 0.5d), Destination.Y - (LineConnecter.Size * 0.5d));
    //        //var bindings = GetSourceDestBindings(converter: new Converters.DoubleSubtractHalfParameterConverter(), converterParameter: (double)LineConnecter.Size);
    //        //BindingOperations.SetBinding(_LineConnecters[0], Canvas.LeftProperty, bindings.SourceX);
    //        //BindingOperations.SetBinding(_LineConnecters[0], Canvas.TopProperty, bindings.SourceY);
    //        //BindingOperations.SetBinding(_LineConnecters[1], Canvas.LeftProperty, bindings.DestinationX);
    //        //BindingOperations.SetBinding(_LineConnecters[1], Canvas.TopProperty, bindings.DestinationY);

    //        canvas.NewLineVisibleThumbs(this);
    //    }
    //    public void MoveLine()
    //    {
    //        HideAllThumbsExceptLineDraggers();
    //        _MiddlePoint = GetMiddlePoint();
    //    }
    //    public void DivideLine()
    //    {
    //        var newLine = new LineBetweenConnectingThumbs(MyConnection)
    //        {
    //            Source = _MiddlePoint,
    //            Destination = Destination,
    //            PreviousLine = this
    //        };
    //        newLine.Draw();
    //        _ArrowLine.ArrowEnds = ArrowEnds.None;

    //        var canvas = ChartCustomControl.Instance;
    //        canvas.ChartCanvas.Children.Add(newLine);
    //        canvas.ChartCanvas.Children.Remove(_LineDivider);
    //        //Destination = _MiddlePoint;

    //        var dragger = new LineDragger(this, _MiddlePoint.X, _MiddlePoint.Y);
    //        canvas.AddElementInCoordinates(dragger, _MiddlePoint.X - (LineDragger.Size * 0.5d), _MiddlePoint.Y - (LineDragger.Size * 0.5d));
    //        //dragger.OnPropertyChanged("AnchorPoint.X");
            
    //        var b = new Binding() { Source = dragger, Path = new PropertyPath(LineDragger.AnchorPointProperty) };
    //        BindingOperations.SetBinding(this, DestinationProperty, b);

    //        if (NextLine == null)
    //        {
    //            _LineDraggers = new LineDragger[2];
    //            newLine._LineDraggers = new LineDragger[2];

    //            _LineDraggers[1] = dragger;
    //            newLine._LineDraggers[0] = dragger;
    //        }
    //        else
    //        {
    //            newLine._LineDraggers = new LineDragger[2];
    //            newLine.NextLine = NextLine;

    //            _LineDraggers[1]._Connection = newLine;
    //            newLine._LineDraggers[0] = dragger;
    //            newLine._LineDraggers[1] = _LineDraggers[1];
    //            _LineDraggers[1] = dragger;
    //        }

    //        //Draw();
    //        NextLine = newLine;
    //    }
    //    #endregion

    //    #region helpers
    //    private Point GetMiddlePoint()
    //    {
    //        return new Point(
    //            (Source.X + Destination.X) * 0.5d,
    //            (Source.Y + Destination.Y) * 0.5d);
    //        //double m = 0.5d * (Destination.X - Source.Y);
    //        //return new Point(Source.X + m, Destination.Y + m);
    //    }
    //    #endregion

    //    #region remove
    //    public void RemoveThis()
    //    {
    //        var undoRedoCommand = ((IChartMainVM)ChartCustomControl.Instance.DataContext).CurrentUndoRedoCommand;
    //        undoRedoCommand.UndoAction += () => ChartCustomControl.Instance.ChartCanvas.Children.Add(this);
    //        undoRedoCommand.RedoAction += () => ChartCustomControl.Instance.ChartCanvas.Children.Remove(this);
    //        ChartCustomControl.Instance.ChartCanvas.Children.Remove(this);

    //        if (!MyConnection.Removed)
    //            MyConnection.RemoveGroup(ref undoRedoCommand);

    //        if (NextLine != null)
    //            NextLine.RemoveThis();

    //        //_LineDraggers = null;
    //        //_LineDivider = null;
    //        //PreviousLine = null;
    //        //NextLine = null;
    //    }
    //    #endregion

    //    #region have thumbs
    //    private bool _ThumbsHidden;
    //    private void HideAllThumbsExceptLineDraggers()
    //    {
    //        foreach (var item in GetAllThumbs())
    //        {
    //            if (item is LineDragger)
    //            {
    //                if (item.Visibility != Visibility.Visible)
    //                    item.Visibility = Visibility.Visible;
    //                continue;
    //            }
    //            if (item.Visibility != Visibility.Hidden)
    //                item.Visibility = Visibility.Hidden;
    //        }
    //        _ThumbsHidden = false;
    //    }
    //    public void HideAllThumbs()
    //    {
    //        if (_ThumbsHidden)
    //            return;
    //        DoActionAllThumbs(x => x.Visibility = Visibility.Hidden);
    //        _ThumbsHidden = true;
    //    }
    //    public void ShowAllThumbs()
    //    {
    //        if (!_ThumbsHidden)
    //            return;

    //        var canvas = ChartCustomControl.Instance;
    //        var middleCanvas = this.TranslatePoint(_MiddlePoint, canvas.ChartCanvas);
    //        var sizeM = LineDivider.Size * 0.5d;
    //        canvas.MoveElementToCoordinates(_LineDivider, middleCanvas.X - sizeM, middleCanvas.Y - sizeM);

    //        _LineMathFunction.Change2Points(Source, Destination);
    //        var margin = Properties.Settings.Default.LinesConnectersYMargin;//_LineMathFunction.Slope > 0 ? Properties.Settings.Default.LinesConnectersYMargin : -1 * Properties.Settings.Default.LinesConnectersYMargin;
    //        var p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Source, margin);
    //        canvas.MoveElementToCoordinates(
    //            _LineConnecters[0],
    //            p.X - (LineConnecter.Size * 0.5d),
    //            p.Y - (LineConnecter.Size * 0.5d));
    //        p = _LineMathFunction.GetPointAlongLineAtDistanceFrom(Destination, -margin);
    //        canvas.MoveElementToCoordinates(
    //            _LineConnecters[1],
    //            p.X - (LineConnecter.Size * 0.5d),
    //            p.Y - (LineConnecter.Size * 0.5d));

    //        DoActionAllThumbs(x => x.Visibility = Visibility.Visible);
    //        _ThumbsHidden = false;
    //    }
    //    public IEnumerable<IChartThumb> GetAllThumbs()
    //    {
    //        if (_LineDraggers != null)
    //        {
    //            if (_LineDraggers[0] != null)
    //                yield return _LineDraggers[0];
    //            if (_LineDraggers[1] != null)
    //                yield return _LineDraggers[1];
    //        }
    //        if (_LineConnecters != null)
    //        {
    //            if (_LineConnecters[0] != null)
    //                yield return _LineConnecters[0];
    //            if (_LineConnecters[1] != null)
    //                yield return _LineConnecters[1];
    //        }
    //        yield return _LineDivider;
    //        yield break;
    //    }
    //    public void DoActionAllThumbs(Action<IChartThumb> action)
    //    {
    //        foreach (var item in GetAllThumbs())
    //        {
    //            action(item);
    //        }
    //    }
    //    #endregion

    //    protected override void UpdateSelectedVisualEffect()
    //    {
    //        if (IsSelected)
    //            _ArrowLine.Stroke = _SelectedBrush;
    //        else
    //            _ArrowLine.Stroke = Brushes.Black;
    //    }
    //}
}
