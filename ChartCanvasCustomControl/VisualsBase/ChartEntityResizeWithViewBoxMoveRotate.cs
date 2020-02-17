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
    public class ChartEntityResizeWithViewBoxMoveRotate : ChartEntityMoveRotate, IVisualResizeMoveRotate
    {
        public virtual FrameworkElement ResizingControl { get; }

        #region move/resize/rotate
        protected static double _MinSize = 40;
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
            if (snap != null)
            {
                if (snap.X.HasValue)
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

                if (snap.Y.HasValue)
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

        #region other visual
        public void OtherVisualFinishResizing()
        {
            _IsResizing = false;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        public void OtherVisualStartResizing()
        {
            _UndoSize = new Size(ActualWidth, ActualHeight);
            _IsResizing = true;
        }
        public void OtherVisualResize(double width, double height)
        {
            ResizingControl.Width = width;
            ResizingControl.Height = height;
            UpdateAnchorPoint();
            CalculateSnapCoords();
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

            var currentMousePositionRelativeToCanvas = e.GetPosition(ChartCustomControl.Instance.ChartCanvas);

            var v = currentMousePositionRelativeToCanvas - _ThumbPositionWhenClickedRelativeToCanvas;

            Console.WriteLine($"{ActualWidth} ; {ActualHeight}");
            Console.WriteLine($"V: {v} ; T: {_ThumbPositionWhenClickedRelativeToCanvas} ; S: {ChartCustomControl.Instance.Scale}");
            ResizingControl.Width = Math.Max(ResizingControl.Width + (v.X * ChartCustomControl.Instance.Scale), _MinSize);
            ResizingControl.Height = Math.Max(ResizingControl.Height + (v.Y * ChartCustomControl.Instance.Scale), _MinSize);

            UpdateAnchorPoint();
            CalculateSnapCoords();

            if (_SelectedBordersWhenResizing != null)
            {
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.OtherVisualResize(
                        Math.Max(item.Visual.ResizingControl.Width + (v.X * ChartCustomControl.Instance.Scale), _MinSize),
                        Math.Max(item.Visual.ResizingControl.Height + (v.Y * ChartCustomControl.Instance.Scale), _MinSize));
                }
            }

            _ThumbPositionWhenClickedRelativeToCanvas = currentMousePositionRelativeToCanvas;
            e.Handled = true;
        }
        internal void ResizeTo(double width, double height)
        {
            _UndoSize = new Size(ResizingControl.Width, ResizingControl.Height);
            var newSize = new Size(width, height);
            var chart = ChartCustomControl.Instance;
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
            AutomaticResizeToWithoutUndoRedo(newSize);
            CreateResizeUndoRedoCommands();
        }
        public void AutomaticResizeToWithoutUndoRedo(Size size)
        {
            ResizingControl.Width = size.Width;
            ResizingControl.Height = size.Height;
            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        #endregion

        #region undo/redo
        private void CreateResizeUndoRedoCommands()
        {
            Action<object[]> undo = x => { };
            object[] parameters;
            Action<object[]> redo = x => { };
            int parsIndex = -1;
            if (_SelectedBordersWhenResizing != null)
            {
                parameters = new object[(_SelectedBordersWhenResizing.Count * 3) + 3];
                foreach (var item in _SelectedBordersWhenResizing)
                {
                    item.Visual.DraggingFinished();

                    parameters[++parsIndex] = item.Visual.UndoSize;
                    parameters[++parsIndex] = item;
                    undo += x => ((SelectedBorderResizingData)x[parsIndex]).Visual.AutomaticResizeToWithoutUndoRedo((Size)x[parsIndex - 1]);
                    parameters[++parsIndex] = new Size(item.Visual.GetUIElement.RenderSize.Width, item.Visual.GetUIElement.RenderSize.Height); //new Point(Canvas.GetLeft(item.Border), Canvas.GetTop(item.Border));
                    redo += x => ((SelectedBorderResizingData)x[parsIndex - 1]).Visual.AutomaticResizeToWithoutUndoRedo((Size)x[parsIndex]);
                }

                _SelectedBordersWhenMoving = null;
            }
            else
                parameters = new object[3];

            parameters[++parsIndex] = new Size(RenderSize.Width, RenderSize.Height); //new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            parameters[++parsIndex] = _UndoSize;
            parameters[++parsIndex] = this;
            undo += x => Console.WriteLine($"*** UNDO resize to = {(Size)x[parsIndex - 1]}");
            undo += x => ((EntityBorderUserControl)x[parsIndex]).AutomaticResizeToWithoutUndoRedo((Size)x[parsIndex - 1]);
            redo += x => Console.WriteLine($"*** REDO resize to = {(Size)x[parsIndex - 2]}");
            redo += x => ((EntityBorderUserControl)x[parsIndex]).AutomaticResizeToWithoutUndoRedo((Size)x[parsIndex - 2]);
            UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.EntityBorder_Resize, undo, parameters, redo, parameters);
        }
        #endregion
    }
}
