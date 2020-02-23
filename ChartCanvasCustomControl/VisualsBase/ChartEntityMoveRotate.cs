using ChartCanvasNamespace.Selection;
using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using UndoRedoSystem;
using ChartCanvasNamespace.OtherVisuals;
using WPFHelpers.CancelActions;

namespace ChartCanvasNamespace.VisualsBase
{
    public class ChartEntityMoveRotate : ChartEntityBorderCanBeSelected, IVisualMoveRotate, IVisualWithSnappingCoordinates
    {
        internal RotateTransform _RotateTransform;

        public virtual Grid BaseRootGrid { get; }
        public virtual EntityMovingThumb BaseMovingThumb { get; }
        public virtual EntityResizingThumb BaseResizingThumb { get; }
        public virtual EntityRotatingThumb BaseRotatingThumb { get; }
        public virtual double CenterX { get; protected set; }
        public virtual double CenterY { get; protected set; }
        public UIElement GetUIElement { get { return this; } }

        #region dependency properties
        public Point Left
        {
            get { return (Point)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(Point), typeof(ChartEntityBorderCanBeSelected), new PropertyMetadata(default(Point)));
        public Point Right
        {
            get { return (Point)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }
        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register("Right", typeof(Point), typeof(ChartEntityBorderCanBeSelected), new PropertyMetadata(default(Point)));
        public Point Top
        {
            get { return (Point)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(Point), typeof(ChartEntityBorderCanBeSelected), new PropertyMetadata(default(Point)));
        public Point Bottom
        {
            get { return (Point)GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.Register("Bottom", typeof(Point), typeof(ChartEntityBorderCanBeSelected), new PropertyMetadata(default(Point)));
        public Thickness ContentMargin
        {
            get { return (Thickness)GetValue(ContentMarginProperty); }
            set { SetValue(ContentMarginProperty, value); }
        }
        public static readonly DependencyProperty ContentMarginProperty =
            DependencyProperty.Register("ContentMargin", typeof(Thickness), typeof(ChartEntityBorderCanBeSelected), new PropertyMetadata(new Thickness(0)));
        public double SelectionBorderThickness
        {
            get { return (double)GetValue(SelectionBorderThicknessProperty); }
            set { SetValue(SelectionBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty SelectionBorderThicknessProperty =
            DependencyProperty.Register("SelectionBorderThickness", typeof(double), typeof(ChartEntityBorderCanBeSelected), new PropertyMetadata(1d));
        #endregion

        internal virtual void CalculateSnapCoords(bool automatic = false) { throw new NotImplementedException(); }
        internal virtual void UpdateAnchorPoint() { throw new NotImplementedException(); }
        public virtual void BindShapeSize() { throw new NotImplementedException(); }

        #region move/rotate
        public class DraggingData
        {
            public Point p;
            public double distanceMovingThumbToCenterX;
            public double distanceMovingThumbToCenterY;
        }
        internal bool _IsMoving;
        internal bool _IsRotating;
        protected Point _ThumbPositionWhenClickedRelativeToCanvas;
        protected Point _ItemCoordsWhenClickedRelativeToCanvas;
        internal Point _UndoMovingCoordinates;
        internal double _UndoAngle;
        protected HashSet<SelectedBorderMovingData> _SelectedBordersWhenMoving = new HashSet<SelectedBorderMovingData>();
        protected HashSet<SelectedBorderRotatingData> _SelectedBordersWhenRotating = new HashSet<SelectedBorderRotatingData>();

        public Point UndoMovingCoordinates { get { return _UndoMovingCoordinates; } }
        public double UndoAngle { get { return _UndoAngle; } }
        public virtual TemporalCurrentSnapCoordinates GetTemporalCurrentSnapCoordinates(Point p) { throw new NotImplementedException(); }
        public virtual void DraggingFinished()
        {
            if (_SelectedBordersWhenMoving != null)
            {
                foreach (var item in _SelectedBordersWhenMoving)
                {
                    item.Visual.OtherVisualFinishMoving();
                }
                _SelectedBordersWhenMoving = null;
            }
            if (_SelectedBordersWhenRotating != null)
            {
                foreach (var item in _SelectedBordersWhenRotating)
                {
                    item.Visual.OtherVisualFinishRotating();
                }
                _SelectedBordersWhenRotating = null;
            }

            _IsMoving = false;
            _IsRotating = false;

            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        protected virtual DraggingData GetDraggingData(Point currentMousePositionRelativeToCanvas)
        {
            var data = new DraggingData();
            data.distanceMovingThumbToCenterX = (BaseRootGrid.ColumnDefinitions[2].ActualWidth * 0.5d) + (BaseRootGrid.ColumnDefinitions[1].ActualWidth * 0.5d);
            data.distanceMovingThumbToCenterY = (BaseRootGrid.RowDefinitions[2].ActualHeight * 0.5d) + (BaseRootGrid.RowDefinitions[1].ActualHeight * 0.5d);

            data.p = new Point(currentMousePositionRelativeToCanvas.X, currentMousePositionRelativeToCanvas.Y);
            data.p.X = data.p.X + _ItemCoordsWhenClickedRelativeToCanvas.X + data.distanceMovingThumbToCenterX - _ThumbPositionWhenClickedRelativeToCanvas.X;
            data.p.Y = data.p.Y + _ItemCoordsWhenClickedRelativeToCanvas.Y + data.distanceMovingThumbToCenterY - _ThumbPositionWhenClickedRelativeToCanvas.Y;

            return data;
        }
        protected virtual void SnapPoint_Moving(ref Point p, SnapToObjectsHandlerClass.SnapTo snap)
        {
            if (snap != null && (snap.Snap || snap.OutOfCanvas))
            {
                if (snap.OutOfCanvasLeft.HasValue || snap.OutOfCanvasRight.HasValue)
                {
                    if (snap.OutOfCanvasLeft.HasValue)
                    {
                        p.X = snap.OutOfCanvasLeft.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth;
                    }
                    else if (snap.OutOfCanvasRight.HasValue)
                    {
                        p.X = snap.OutOfCanvasRight.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - BaseRootGrid.ColumnDefinitions[1].ActualWidth - ContentMargin.Right;
                    }
                }
                else if (snap.X.HasValue)
                {
                    switch (snap.XType)
                    {
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Center:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - (BaseRootGrid.ColumnDefinitions[1].ActualWidth * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Left:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Right:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - BaseRootGrid.ColumnDefinitions[1].ActualWidth - ContentMargin.Right;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThLeft:
                            p.X = snap.X.Value - (BaseRootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThRight:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - BaseRootGrid.ColumnDefinitions[1].ActualWidth
                                - ContentMargin.Right - (BaseRootGrid.ColumnDefinitions[2].ActualWidth * 0.5d);
                            break;
                    }
                    p.X += SelectionBorderThickness;
                }

                if (snap.OutOfCanvasTop.HasValue || snap.OutOfCanvasBottom.HasValue)
                {
                    if (snap.OutOfCanvasTop.HasValue)
                    {
                        Console.WriteLine($@"
----------------------------------------------
Out top: {snap.OutOfCanvasTop.Value} 
old p.y: {p.Y}");
                        p.Y = snap.OutOfCanvasTop.Value - BaseRootGrid.RowDefinitions[0].ActualHeight;
                        Console.WriteLine($@"new p.y: {p.Y}");
                    }
                    else if (snap.OutOfCanvasBottom.HasValue)
                    {
                        p.Y = snap.OutOfCanvasBottom.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - BaseRootGrid.RowDefinitions[1].ActualHeight - ContentMargin.Bottom;
                    }
                }
                else if (snap.Y.HasValue)
                {
                    switch (snap.YType)
                    {
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Center:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - (BaseRootGrid.RowDefinitions[1].ActualHeight * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Top:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Bottom:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - BaseRootGrid.RowDefinitions[1].ActualHeight - ContentMargin.Bottom;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThTop:
                            p.Y = snap.Y.Value - (BaseRootGrid.RowDefinitions[0].ActualHeight * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThBottom:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - BaseRootGrid.RowDefinitions[1].ActualHeight
                                - ContentMargin.Bottom - (BaseRootGrid.RowDefinitions[2].ActualHeight * 0.5d);
                            break;
                    }
                    p.Y += SelectionBorderThickness;
                }

            }
        }

        #region other visual
        public void OtherVisualFinishMoving()
        {
            _IsMoving = false;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        public void OtherVisualFinishRotating()
        {
            _IsRotating = false;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        public void OtherVisualStartMoving()
        {
            _UndoMovingCoordinates = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            _IsMoving = true;
        }
        public void OtherVisualStartRotating()
        {
            _UndoAngle = _RotateTransform.Angle;
            _IsRotating = true;
        }
        public void OtherVisualMove(double x, double y)
        {
            var currentPosition = new Point(x, y);
            var delta = ChartCustomControl.Instance.SnapToObjectsHandler.CheckOutOfCanvasAndReturnPositionDelta(GetTemporalCurrentSnapCoordinates(currentPosition));
            if (delta.X != 0 || delta.Y != 0)
            {
                AutomaticMoveToWithoutUndoRedo(new Point(currentPosition.X + delta.X, currentPosition.Y + delta.Y));
            }
            else
            {
                AutomaticMoveToWithoutUndoRedo(currentPosition);
            }
        }
        public void OtherVisualRotate(double angle)
        {
            _RotateTransform.Angle = angle;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        public void OtherVisualFastRotate(double angle)
        {
            _UndoAngle = _RotateTransform.Angle;
            OtherVisualRotate(angle);
        }
        #endregion

        private void MovingThumb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _IsMoving = true;
            _UndoMovingCoordinates = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            _ThumbPositionWhenClickedRelativeToCanvas = BaseMovingThumb.GetAnchorPoint();
            _ItemCoordsWhenClickedRelativeToCanvas = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            ((EntityMovingThumb)sender).CaptureMouse();

            var chart = ChartCustomControl.Instance;
            var selectedCount = chart.ChartEntitiesSelected.Count + chart.VisualTextsSelected.Count;
            if (selectedCount == 1)
            {
                if ((chart.ChartEntitiesSelected.Count > 0 && !chart.ChartEntitiesSelected[0].Equals(DataContext)) ||
                    (chart.VisualTextsSelected.Count > 0 && !chart.VisualTextsSelected[0].Equals(DataContext)))
                    chart.ChartItemsSelectionHandler.ClearItemsSelected();
            }
            else if (selectedCount > 1)
            {
                _SelectedBordersWhenMoving = chart.ChartItemsSelectionHandler.GetSelectedBordersMovingCurrentPosition(this);
                foreach (var item in _SelectedBordersWhenMoving)
                {
                    item.Visual.OtherVisualStartMoving();
                    item.SetDistanceBetweenOriginalAndThis(this);
                }
            }

            e.Handled = true;
        }
        private void MovingThumb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _ThumbPositionWhenClickedRelativeToCanvas = default(Point);
            ((EntityMovingThumb)sender).ReleaseMouseCapture();
            //onLayoutUpdated(null, null);
            _ItemCoordsWhenClickedRelativeToCanvas = default(Point);

            ChartCustomControl.Instance.SnapToObjectsHandler.HidSnapLines();
            CreateMoveUndoRedoCommands();
            DraggingFinished();

            e.Handled = true;
        }
        private void MovingThumb_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsMoving)
                return;

            var currentMousePositionRelativeToCanvas = e.GetPosition(ChartCustomControl.Instance.ChartCanvas);
            var data = GetDraggingData(currentMousePositionRelativeToCanvas);

            var tempCoords = GetTemporalCurrentSnapCoordinates(data.p);

            var snap = ChartCustomControl.Instance.SnapToObjectsHandler.CheckPointShouldSnap(this, tempCoords);

            SnapPoint_Moving(ref data.p, snap);

            Canvas.SetLeft(this, data.p.X);
            Canvas.SetTop(this, data.p.Y);

            UpdateAnchorPoint();
            CalculateSnapCoords();

            if (_SelectedBordersWhenMoving != null)
            {
                foreach (var item in _SelectedBordersWhenMoving)
                {
                    double x = data.p.X + item.Distance.X;
                    double y = data.p.Y + item.Distance.Y;

                    item.Visual.OtherVisualMove(x, y);
                }
            }

            e.Handled = true;
        }
        public void AutomaticMoveToWithoutUndoRedo(Point p)
        {
            Console.WriteLine($"Automatic move to = {p}");
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
            UpdateAnchorPoint();
            CalculateSnapCoords(true);
        }

        private void RotatingThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            Console.WriteLine("drag start");
            _IsRotating = true;
            _UndoAngle = _RotateTransform.Angle;
            _ThumbPositionWhenClickedRelativeToCanvas = BaseRotatingThumb.GetAnchorPoint();
            //RotatingThumb.CaptureMouse();

            var chart = ChartCustomControl.Instance;
            var selectedCount = chart.ChartEntitiesSelected.Count + chart.VisualTextsSelected.Count;
            if (selectedCount == 1)
            {
                if ((chart.ChartEntitiesSelected.Count > 0 && !chart.ChartEntitiesSelected[0].Equals(DataContext)) ||
                    (chart.VisualTextsSelected.Count > 0 && !chart.VisualTextsSelected[0].Equals(DataContext)))
                    chart.ChartItemsSelectionHandler.ClearItemsSelected();
            }
            else if (selectedCount > 1)
            {
                _SelectedBordersWhenRotating = chart.ChartItemsSelectionHandler.GetSelectedBordersRotatingCurrentAngle(this);
                foreach (var item in _SelectedBordersWhenRotating)
                {
                    item.Visual.OtherVisualStartRotating();
                }
            }
        }
        private void RotatingThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!_IsRotating)
                return;

            Console.WriteLine("drag completed");
            ChartCustomControl.Instance.SnapToObjectsHandler.HidSnapLines();
            if (_RotateTransform.Angle != _UndoAngle)
                CreateRotateUndoRedoCommands();
            DraggingFinished();

            _ThumbPositionWhenClickedRelativeToCanvas = default(Point);
            e.Handled = true;
        }
        private void RotatingThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (!_IsRotating)
                return;

            //Console.WriteLine($"HC {e.HorizontalChange}");
            var pos = Mouse.GetPosition(ChartCustomControl.Instance.ChartCanvas);
            double angle = (pos.X - _ThumbPositionWhenClickedRelativeToCanvas.X) * ChartCustomControl.Instance.Scale;
            Console.WriteLine($"Angle {angle}");
            //_RotateTransform.Angle += (e.HorizontalChange * ChartCustomControl.Instance.Scale);
            _RotateTransform.Angle = angle;

            UpdateAnchorPoint();
            CalculateSnapCoords();

            if (_SelectedBordersWhenRotating != null)
            {
                foreach (var item in _SelectedBordersWhenRotating)
                {
                    item.Visual.OtherVisualRotate(angle);
                }
            }

            e.Handled = true;
        }
        private void RotatingThumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_RotateTransform.Angle == 0)
                return;

            RotateTo(0);

            e.Handled = true;
        }
        public void RotateTo(double angle)
        {
            _UndoAngle = _RotateTransform.Angle;
            var chart = ChartCustomControl.Instance;
            if (chart.ChartEntitiesSelected.Count == 1)
            {
                if (!chart.ChartEntitiesSelected[0].Equals(DataContext))
                    chart.ChartItemsSelectionHandler.ClearItemsSelected();
            }
            else if (chart.ChartEntitiesSelected.Count > 1)
            {
                _SelectedBordersWhenRotating = chart.ChartItemsSelectionHandler.GetSelectedBordersRotatingCurrentAngle(this);
                foreach (var item in _SelectedBordersWhenRotating)
                {
                    item.Visual.OtherVisualRotate(angle);
                }
            }
            AutomaticRotateToWithoutUndoRedo(angle);
            CreateRotateUndoRedoCommands();
        }
        public void AutomaticRotateToWithoutUndoRedo(double angle)
        {
            _RotateTransform.Angle = angle;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        #endregion

        #region undo/redo
        private void CreateMoveUndoRedoCommands()
        {
            Action<object[]> undo = x => { };
            object[] parameters;
            Action<object[]> redo = x => { };
            int parsIndex = -1;
            if (_SelectedBordersWhenMoving != null)
            {
                parameters = new object[(_SelectedBordersWhenMoving.Count * 3) + 3];
                foreach (var item in _SelectedBordersWhenMoving)
                {
                    item.Visual.DraggingFinished();

                    parameters[++parsIndex] = item.Visual.UndoMovingCoordinates;
                    parameters[++parsIndex] = item;
                    var itemIndex = parsIndex;
                    parameters[++parsIndex] = new Point(Canvas.GetLeft(item.Visual.GetUIElement), Canvas.GetTop(item.Visual.GetUIElement));
                    undo += x => ((SelectedBorderMovingData)x[itemIndex]).Visual.AutomaticMoveToWithoutUndoRedo((Point)x[itemIndex - 1]);
                    redo += x => ((SelectedBorderMovingData)x[itemIndex]).Visual.AutomaticMoveToWithoutUndoRedo((Point)x[itemIndex + 1]);
                }

                _SelectedBordersWhenMoving = null;
            }
            else
                parameters = new object[3];
            Console.WriteLine($"* Creating undo/redo command:_UndoMovingCoordinates = {_UndoMovingCoordinates}{Environment.NewLine}  ;  Last coordinates = {new Point(Canvas.GetLeft(this), Canvas.GetTop(this))}");
            parameters[++parsIndex] = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            parameters[++parsIndex] = _UndoMovingCoordinates;
            parameters[++parsIndex] = this;
            undo += x => Console.WriteLine($"*** UNDO move to = {(Point)x[parsIndex - 1]}");
            undo += x => ((ChartEntityMoveRotate)x[parsIndex]).AutomaticMoveToWithoutUndoRedo((Point)x[parsIndex - 1]);
            redo += x => Console.WriteLine($"*** REDO move to = {(Point)x[parsIndex - 2]}");
            redo += x => ((ChartEntityMoveRotate)x[parsIndex]).AutomaticMoveToWithoutUndoRedo((Point)x[parsIndex - 2]);
            UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.EntityBorder_Move, undo, parameters, redo, parameters);
        }
        private void CreateRotateUndoRedoCommands()
        {
            Action<object[]> undo = x => { };
            object[] parameters;
            Action<object[]> redo = x => { };
            int parsIndex = -1;
            if (_SelectedBordersWhenRotating != null)
            {
                parameters = new object[(_SelectedBordersWhenRotating.Count * 3) + 3];
                foreach (var item in _SelectedBordersWhenRotating)
                {
                    item.Visual.DraggingFinished();

                    parameters[++parsIndex] = item.Visual.UndoAngle;
                    parameters[++parsIndex] = item;
                    var itemIndex = parsIndex;
                    parameters[++parsIndex] = _RotateTransform.Angle;
                    undo += x => ((SelectedBorderRotatingData)x[itemIndex]).Visual.AutomaticRotateToWithoutUndoRedo((double)x[itemIndex - 1]);
                    redo += x => ((SelectedBorderRotatingData)x[itemIndex]).Visual.AutomaticRotateToWithoutUndoRedo((double)x[itemIndex + 1]);
                }

                _SelectedBordersWhenMoving = null;
            }
            else
                parameters = new object[3];

            parameters[++parsIndex] = _RotateTransform.Angle;
            parameters[++parsIndex] = _UndoAngle;
            parameters[++parsIndex] = this;
            undo += x => Console.WriteLine($"*** UNDO rotation to = {(double)x[parsIndex - 1]}");
            undo += x => ((ChartEntityMoveRotate)x[parsIndex]).AutomaticRotateToWithoutUndoRedo((double)x[parsIndex - 1]);
            redo += x => Console.WriteLine($"*** REDO rotation to = {(double)x[parsIndex - 2]}");
            redo += x => ((ChartEntityMoveRotate)x[parsIndex]).AutomaticRotateToWithoutUndoRedo((double)x[parsIndex - 2]);
            UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.EntityBorder_Rotation, undo, parameters, redo, parameters);
        }
        #endregion

        #region equatable
        public virtual bool Equals(IVisualMoveRotate other) { throw new NotImplementedException(); }
        public virtual bool Equals(IVisualWithSnappingCoordinates other) { throw new NotImplementedException(); }
        int IVisualWithSnappingCoordinates.GetHashCode() { return GetVisualHashCode(); }
        int IVisualMoveRotate.GetHashCode() { return GetVisualHashCode(); }
        public virtual int GetVisualHashCode() { throw new NotImplementedException(); }
        #endregion
    }
}
