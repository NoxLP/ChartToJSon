using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UndoRedoSystem;

namespace ChartCanvasNamespace.VisualsBase
{
    public class ChartEntityResizeMoveRotate : ChartEntityMoveRotate, IVisualResizeMoveRotate
    {
        public virtual FrameworkElement ResizingControl { get; }

        #region resize
        internal bool _IsAutoResizing;
        internal bool _IsResizing;
        internal Size _UndoSize;
        protected HashSet<SelectedBorderResizingData> _SelectedBordersWhenResizing = new HashSet<SelectedBorderResizingData>();

        public Size UndoSize { get { return _UndoSize; } }

        public override void DraggingFinished()
        {
            if (_SelectedBordersWhenMoving != null)
            {
                foreach (var item in _SelectedBordersWhenMoving)
                {
                    item.Visual.OtherVisualFinishMoving();
                }
                _SelectedBordersWhenMoving = null;
            }
            if (_SelectedBordersWhenResizing != null)
            {
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.OtherVisualFinishResizing();
                }
                _SelectedBordersWhenResizing = null;
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
            _IsResizing = false;
            _IsRotating = false;

            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        protected override DraggingData GetDraggingData(Point currentMousePositionRelativeToCanvas)
        {
            var data = new DraggingData();
            data.distanceMovingThumbToCenterX = (BaseRootGrid.ColumnDefinitions[2].ActualWidth * 0.5d) + (ResizingControl.Width * 0.5d);
            data.distanceMovingThumbToCenterY = (BaseRootGrid.RowDefinitions[2].ActualHeight * 0.5d) + (ResizingControl.Height * 0.5d);

            data.p = new Point(currentMousePositionRelativeToCanvas.X, currentMousePositionRelativeToCanvas.Y);
            data.p.X = data.p.X + _ItemCoordsWhenClickedRelativeToCanvas.X + data.distanceMovingThumbToCenterX - _ThumbPositionWhenClickedRelativeToCanvas.X;
            data.p.Y = data.p.Y + _ItemCoordsWhenClickedRelativeToCanvas.Y + data.distanceMovingThumbToCenterY - _ThumbPositionWhenClickedRelativeToCanvas.Y;

            return data;
        }
        protected override void SnapPoint_Moving(ref Point p, SnapToObjectsHandlerClass.SnapTo snap)
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
                        p.X = snap.OutOfCanvasRight.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - ResizingControl.Width - ContentMargin.Right;
                    }
                }
                else if (snap.X.HasValue)
                {
                    switch (snap.XType)
                    {
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Center:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - (ResizingControl.Width * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Left:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Right:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - ResizingControl.Width - ContentMargin.Right;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThLeft:
                            p.X = snap.X.Value - (BaseRootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThRight:
                            p.X = snap.X.Value - BaseRootGrid.ColumnDefinitions[0].ActualWidth - ContentMargin.Left - ResizingControl.Width
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
                        p.Y = snap.OutOfCanvasBottom.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - ResizingControl.Height - ContentMargin.Bottom;
                    }
                }
                else if (snap.Y.HasValue)
                {
                    switch (snap.YType)
                    {
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Center:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - (ResizingControl.Height * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Top:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.Bottom:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - ResizingControl.Height - ContentMargin.Bottom;
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThTop:
                            p.Y = snap.Y.Value - (BaseRootGrid.RowDefinitions[0].ActualHeight * 0.5d);
                            break;
                        case SnapToObjectsHandlerClass.SnapBorderTypeEnum.ThBottom:
                            p.Y = snap.Y.Value - BaseRootGrid.RowDefinitions[0].ActualHeight - ContentMargin.Top - ResizingControl.Height
                                - ContentMargin.Bottom - (BaseRootGrid.RowDefinitions[2].ActualHeight * 0.5d);
                            break;
                    }
                    p.Y += SelectionBorderThickness;
                }

            }
        }
        protected virtual double MinSize => 40;

        #region other visual
        public void OtherVisualFinishResizing()
        {
            _IsResizing = false;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        public void OtherVisualStartResizing()
        {
            _UndoSize = new Size(ResizingControl.Width, ResizingControl.Height);
            _ItemCoordsWhenClickedRelativeToCanvas = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            _IsResizing = true;
        }
        public void OtherVisualResize(double width, double height)
        {
            ResizingControl.Width = width;
            ResizingControl.Height = height;
            UpdateAnchorPoint();
            CalculateSnapCoords();

            var currentPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            var delta = ChartCustomControl.Instance.SnapToObjectsHandler.CheckOutOfCanvasAndReturnPositionDelta(GetTemporalCurrentSnapCoordinates(currentPosition));
            if (delta.X != 0 || delta.Y != 0)
            {
                AutomaticMoveToWithoutUndoRedo(new Point(currentPosition.X + delta.X, currentPosition.Y + delta.Y));
                UpdateAnchorPoint();
                CalculateSnapCoords();
            }
        }
        public void OtherVisualFastResize(double width, double height)
        {
            _UndoSize = new Size(ResizingControl.Width, ResizingControl.Height);
            OtherVisualResize(width, height);
        }
        #endregion

        private void ResizingThumb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _IsResizing = true;
            _UndoSize = new Size(ResizingControl.Width, ResizingControl.Height);
            _ThumbPositionWhenClickedRelativeToCanvas = BaseResizingThumb.GetAnchorPoint();
            _ItemCoordsWhenClickedRelativeToCanvas = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            ((EntityResizingThumb)sender).CaptureMouse();

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
                _SelectedBordersWhenResizing = chart.ChartItemsSelectionHandler.GetSelectedBordersResizingCurrentSize(this);
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.OtherVisualStartResizing();
                    item.SetDistanceBetweenOriginalAndThis(this);
                }
            }

            e.Handled = true;
        }
        private void ResizingThumb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_SelectedBordersWhenResizing != null)
            {
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.BindShapeSize();
                }
            }
            ((EntityResizingThumb)sender).ReleaseMouseCapture();

            ChartCustomControl.Instance.SnapToObjectsHandler.HidSnapLines();
            CreateResizeUndoRedoCommands();
            DraggingFinished();

            _ThumbPositionWhenClickedRelativeToCanvas = default(Point);
            e.Handled = true;
        }
        private void ResizingThumb_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsResizing)
                return;

            var chart = ChartCustomControl.Instance;
            var currentMousePositionRelativeToCanvas = e.GetPosition(chart.ChartCanvas);

            var v = currentMousePositionRelativeToCanvas - _ThumbPositionWhenClickedRelativeToCanvas;

            Console.WriteLine($"{ActualWidth} ; {ActualHeight}");
            Console.WriteLine($"V: {v} ; T: {_ThumbPositionWhenClickedRelativeToCanvas} ; S: {chart.Scale}");
            ResizingControl.Width = Math.Max(ResizingControl.Width + (v.X * chart.Scale), MinSize);
            ResizingControl.Height = Math.Max(ResizingControl.Height + (v.Y * chart.Scale), MinSize);

            UpdateAnchorPoint();
            CalculateSnapCoords();

            var currentPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            var delta = chart.SnapToObjectsHandler.CheckOutOfCanvasAndReturnPositionDelta(GetTemporalCurrentSnapCoordinates(currentPosition));
            if (delta.X != 0 || delta.Y != 0)
            {
                AutomaticMoveToWithoutUndoRedo(new Point(currentPosition.X + delta.X, currentPosition.Y + delta.Y));
                UpdateAnchorPoint();
                CalculateSnapCoords();
            }

            if (_SelectedBordersWhenResizing != null)
            {
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.OtherVisualResize(
                        Math.Max(item.Visual.ResizingControl.Width + (v.X * ChartCustomControl.Instance.Scale), MinSize),
                        Math.Max(item.Visual.ResizingControl.Height + (v.Y * ChartCustomControl.Instance.Scale), MinSize));
                }
            }

            _ThumbPositionWhenClickedRelativeToCanvas = currentMousePositionRelativeToCanvas;
            e.Handled = true;
        }
        internal void ResizeTo(double width, double height)
        {
            _IsAutoResizing = true;
            _UndoSize = new Size(ResizingControl.Width, ResizingControl.Height);
            _ItemCoordsWhenClickedRelativeToCanvas = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

            var newSize = new Size(width, height);
            AutomaticResizeToWithoutUndoRedo(newSize);

            var chart = ChartCustomControl.Instance;
            var currentPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            var delta = chart.SnapToObjectsHandler.CheckOutOfCanvasAndReturnPositionDelta(GetTemporalCurrentSnapCoordinates(currentPosition));
            if (delta.X != 0 || delta.Y != 0)
            {
                AutomaticMoveToWithoutUndoRedo(new Point(currentPosition.X + delta.X, currentPosition.Y + delta.Y));
                UpdateAnchorPoint();
                CalculateSnapCoords();
            }

            if (chart.ChartEntitiesSelected.Count == 1)
            {
                if (!chart.ChartEntitiesSelected[0].Equals(DataContext))
                    chart.ChartItemsSelectionHandler.ClearItemsSelected();
            }
            else if (chart.ChartEntitiesSelected.Count > 1)
            {
                _SelectedBordersWhenResizing = chart.ChartItemsSelectionHandler.GetSelectedBordersResizingCurrentSize(this);
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.OtherVisualFastResize(width, height);
                }
            }
            
            CreateResizeUndoRedoCommands();
        }
        public void AutomaticResizeToWithoutUndoRedo(Size size)
        {
            _IsAutoResizing = true;
            ResizingControl.Width = size.Width;
            ResizingControl.Height = size.Height;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        #endregion

        #region undo/redo
        private object[] _Parameters;
        private int _ParsIndex = -1;
        private Action<object[]> _Undo = x => { };
        private Action<object[]> _Redo = x => { };

        private void SetThisUndoRedoMoved()
        {
            _Parameters[++_ParsIndex] = this;
            int itemIndex1 = _ParsIndex;
            _Parameters[++_ParsIndex] = _UndoSize;
            _Parameters[++_ParsIndex] = new Size(ResizingControl.Width, ResizingControl.Height);

            _Undo += x => Console.WriteLine($"*** UNDO resize to = {(Size)x[itemIndex1 + 1]}");
            _Undo += x => ((ChartEntityResizeMoveRotate)x[itemIndex1]).AutomaticResizeToWithoutUndoRedo((Size)x[itemIndex1 + 1]);
            _Redo += x => Console.WriteLine($"*** REDO resize to = {(Size)x[itemIndex1 + 2]}");
            _Redo += x => ((ChartEntityResizeMoveRotate)x[itemIndex1]).AutomaticResizeToWithoutUndoRedo((Size)x[itemIndex1 + 2]);

            _Parameters[++_ParsIndex] = _ItemCoordsWhenClickedRelativeToCanvas;
            int closureProxy1 = _ParsIndex;
            _Undo += x => ((ChartEntityResizeMoveRotate)x[itemIndex1]).AutomaticMoveToWithoutUndoRedo((Point)x[closureProxy1]);
            _Parameters[++_ParsIndex] = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            int closureProxy2 = _ParsIndex;
            _Redo += x => ((ChartEntityResizeMoveRotate)x[itemIndex1]).AutomaticMoveToWithoutUndoRedo((Point)x[closureProxy2]);
        }
        private void SetThisUndoRedoNotMoved()
        {
            _Parameters[++_ParsIndex] = this;
            int itemIndex3 = _ParsIndex;
            _Parameters[++_ParsIndex] = _UndoSize;
            _Parameters[++_ParsIndex] = new Size(ResizingControl.Width, ResizingControl.Height);

            _Undo += x => Console.WriteLine($"*** UNDO resize to = {(Size)x[itemIndex3 + 1]}");
            _Undo += x => ((ChartEntityResizeMoveRotate)x[itemIndex3]).AutomaticResizeToWithoutUndoRedo((Size)x[itemIndex3 + 1]);
            _Redo += x => Console.WriteLine($"*** REDO resize to = {(Size)x[itemIndex3 + 2]}");
            _Redo += x => ((ChartEntityResizeMoveRotate)x[itemIndex3]).AutomaticResizeToWithoutUndoRedo((Size)x[itemIndex3 + 2]);
        }
        private void CreateResizeUndoRedoCommands()
        {
            _Undo = x => { };
            _Redo = x => { };
            var currentPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            bool moved = !currentPosition.Equals(_ItemCoordsWhenClickedRelativeToCanvas);

            if (_SelectedBordersWhenResizing != null)
            {
                var itemsParams = new List<object>();
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.DraggingFinished();
                    var uiElem = item.Visual.GetUIElement;

                    itemsParams.Add(item); //item index
                    ++_ParsIndex;
                    int itemIndex = _ParsIndex; // -> closure proxy
                    itemsParams.Add(item.Visual.UndoSize);
                    itemsParams.Add(new Size(item.Visual.ResizingControl.Width, item.Visual.ResizingControl.Height));
                    _ParsIndex += 2;
                    _Undo += x => ((SelectedBorderResizingData)x[itemIndex]).Visual.AutomaticResizeToWithoutUndoRedo((Size)x[itemIndex + 1]);
                    _Redo += x => ((SelectedBorderResizingData)x[itemIndex]).Visual.AutomaticResizeToWithoutUndoRedo((Size)x[itemIndex + 2]);

                    var itemCurrentPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
                    if (!itemCurrentPosition.Equals(item.ItemCoordsWhenClickedRelativeToCanvas))
                    {
                        itemsParams.Add(item.ItemCoordsWhenClickedRelativeToCanvas);
                        itemsParams.Add(new Point(Canvas.GetLeft(uiElem), Canvas.GetTop(uiElem)));
                        _ParsIndex += 2;

                        _Undo += x => ((SelectedBorderResizingData)x[itemIndex]).Visual.AutomaticMoveToWithoutUndoRedo((Point)x[itemIndex + 3]);
                        _Redo += x => ((SelectedBorderResizingData)x[itemIndex]).Visual.AutomaticMoveToWithoutUndoRedo((Point)x[itemIndex + 4]);
                    }
                }

                _SelectedBordersWhenMoving = null;
                if (moved)
                {
                    _Parameters = new object[itemsParams.Count + 5];
                    itemsParams.CopyTo(_Parameters);
                    SetThisUndoRedoMoved();
                }
                else
                {
                    _Parameters = new object[itemsParams.Count + 3];
                    itemsParams.CopyTo(_Parameters);
                    SetThisUndoRedoNotMoved();
                }
            }
            else
            {
                if (moved)
                {
                    _Parameters = new object[5];
                    SetThisUndoRedoMoved();
                }
                else
                {
                    _Parameters = new object[3];
                    SetThisUndoRedoNotMoved();
                }
            }

            UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.EntityBorder_Resize, _Undo, _Parameters, _Redo, _Parameters);
            _Undo = null;
            _Redo = null;
            _ParsIndex = -1;
            _Parameters = null;
        }
        #endregion
    }
}
