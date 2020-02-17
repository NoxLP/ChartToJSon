using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Entities.EntitiesShapesUserControls;
using ChartCanvasNamespace.Lines;
using ChartCanvasNamespace.OtherVisuals;
using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ChartCanvasNamespace.Selection
{
    public class ChartItemsSelectionHandlerClass : IChartItemsSelectionHandler
    {
        public ChartItemsSelectionHandlerClass(ChartCustomControl chart)
        {
            Chart = chart;
        }
        //https://stackoverflow.com/questions/16859865/wpf-mutliselection-of-shapes-using-drag-rectangle
        //https://stackoverflow.com/questions/1838163/click-and-drag-selection-box-in-wpf
        //https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/hit-testing-in-the-visual-layer?redirectedfrom=MSDN

        //private bool _MouseDown = false;
        //private Point _MouseDownPos;

        private ChartCustomControl Chart;

        #region selection box
        public void InitDragSelectionBox(Point pt1, Point pt2)
        {
            SetDragSelectionBox(pt1, pt2);

            Chart._SelectionBoxCanvas.Visibility = Visibility.Visible;
            Chart._SelectionBox.Opacity = 0.5d;
        }
        public void SetDragSelectionBox(Point pt1, Point pt2)
        {
            var data = new ChartCustomControl.DragBoxesData(pt1, pt2);

            //
            // Update the coordinates of the rectangle that is being dragged out by the user.
            // The we offset and rescale to convert from content coordinates.
            //
            Canvas.SetLeft(Chart._SelectionBox, data.x);
            Canvas.SetTop(Chart._SelectionBox, data.y);
            Chart._SelectionBox.Width = data.width;
            Chart._SelectionBox.Height = data.height;
        }
        public void ApplyDragSelectionBox()
        {
            var rectGeometry = new RectangleGeometry(new Rect(
                Canvas.GetLeft(Chart._SelectionBox),
                Canvas.GetTop(Chart._SelectionBox),
                Chart._SelectionBox.ActualWidth,
                Chart._SelectionBox.ActualHeight));
            var hits = new HashSet<IVisualCanBeSelected>();
            VisualTreeHelper.HitTest(
                Chart.ChartCanvas, null,
                //x =>
                //{
                //    if (!typeof(IVisualCanBeSelected).IsAssignableFrom(x.GetType()))
                //        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                //    return HitTestFilterBehavior.Continue;
                //},
                x =>
                {
                    var element = x.VisualHit;
                    while (element != null && !typeof(IVisualCanBeSelected).IsAssignableFrom(element.GetType()))
                        element = VisualTreeHelper.GetParent(element);

                    var hit = element as IVisualCanBeSelected;
                    if (hit != null && !hits.Contains(hit))
                        hits.Add(hit);
                    //hits.Add((IVisualCanBeSelected)x.VisualHit);
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(rectGeometry));

            ItemsSelectedFromSelectionBox(hits);

            FadeOutDragSelectionBox();
        }
        public void FadeOutDragSelectionBox()
        {
            ChartCustomControl.Instance.StartAnimation(Chart._SelectionBox, Border.OpacityProperty, 0.0, 0.1,
                delegate (object sender, EventArgs e)
                {
                    Chart._SelectionBoxCanvas.Visibility = Visibility.Collapsed;
                });
        }
        #endregion

        #region items selection
        private void AddItemSelected(IVisualCanBeSelected selected)
        {
            selected.IsSelected = true;

            switch(selected.Type)
            {
                case CanBeSelectedItemTypeEnum.EntityBorder:
                    var border = selected as EntityBorderUserControl;
                    if (border != null)
                    {
                        var bvm = (IChartEntityViewModel)border.DataContext;
                        if (!Chart.ChartEntitiesSelected.Contains(bvm))
                            Chart.ChartEntitiesSelected.Add(bvm);
                    }
                    else
                    {
                        var uc = selected as ChartEntityUserControl;
                        var evm = (IChartEntityViewModel)uc.DataContext;
                        if (!Chart.ChartEntitiesSelected.Contains(evm))
                            Chart.ChartEntitiesSelected.Add(evm);
                    }
                    break;
                case CanBeSelectedItemTypeEnum.LineConnection:
                case CanBeSelectedItemTypeEnum.Thumb:
                    if (!Chart.VisualItemsSelected.Contains(selected))
                        Chart.VisualItemsSelected.Add(selected);
                    break;
                case CanBeSelectedItemTypeEnum.Text:
                case CanBeSelectedItemTypeEnum.TextWithShape:
                    var text = selected as ChartEntityResizeMoveRotate;
                    var tvm = (IVisualTextViewModel)text.DataContext;
                    if (!Chart.VisualTextsSelected.Contains(tvm))
                        Chart.VisualTextsSelected.Add(tvm);
                    break;
            }
        }
        private void RemoveItemSelected(IVisualCanBeSelected selected)
        {
            selected.IsSelected = false;

            switch (selected.Type)
            {
                case CanBeSelectedItemTypeEnum.EntityBorder:
                    var border = selected as EntityBorderUserControl;
                    if (border != null)
                    {
                        Chart.ChartEntitiesSelected.Remove((IChartEntityViewModel)border.DataContext);
                    }
                    else
                    {
                        var uc = selected as ChartEntityUserControl;
                        Chart.ChartEntitiesSelected.Remove((IChartEntityViewModel)uc.DataContext);
                    }
                    break;
                case CanBeSelectedItemTypeEnum.LineConnection:
                case CanBeSelectedItemTypeEnum.Thumb:
                    Chart.VisualItemsSelected.Remove(selected);
                    break;
                case CanBeSelectedItemTypeEnum.Text:
                case CanBeSelectedItemTypeEnum.TextWithShape:
                    var text = selected as ChartEntityResizeMoveRotate;
                    Chart.VisualTextsSelected.Remove((IVisualTextViewModel)text.DataContext);
                    break;
            }
        }
        private bool ItemAlreadySelected(IVisualCanBeSelected selected)
        {
            switch (selected.Type)
            {
                case CanBeSelectedItemTypeEnum.EntityBorder:
                    var border = selected as EntityBorderUserControl;
                    IChartEntityViewModel cvm;
                    if (border != null)
                    {
                        cvm = (IChartEntityViewModel)border.DataContext;
                    }
                    else
                    {
                        var uc = selected as ChartEntityUserControl;
                        cvm = (IChartEntityViewModel)uc.DataContext;
                    }
                    return Chart.ChartEntitiesSelected.Contains(cvm);
                case CanBeSelectedItemTypeEnum.LineConnection:
                case CanBeSelectedItemTypeEnum.Thumb:
                    return Chart.VisualItemsSelected.Contains(selected);
                    break;
                case CanBeSelectedItemTypeEnum.Text:
                case CanBeSelectedItemTypeEnum.TextWithShape:
                    var text = selected as ChartEntityResizeMoveRotate;
                    return Chart.VisualTextsSelected.Contains((IVisualTextViewModel)text.DataContext);
                    break;
            }
            return false;
        }
        public void ItemSelected(IVisualCanBeSelected selected)
        {
            if ((selected.ModifiersWhenSelectingSelf & ModifierKeys.Shift) != 0 || (selected.ModifiersWhenSelectingSelf & ModifierKeys.Control) != 0)
            {
                if (ItemAlreadySelected(selected))
                    RemoveItemSelected(selected);
                else
                    AddItemSelected(selected);
            }
            else
            {
                if (selected.IsSelected)
                {
                    var toRemove = new List<IVisualCanBeSelected>(Chart.VisualItemsSelected);
                    foreach (var visual in toRemove)
                    {
                        if (!visual.Equals(selected))
                            RemoveItemSelected(visual);
                    }
                }
                else
                {
                    ClearItemsSelected();
                    AddItemSelected(selected);
                }
            }
        }
        public void ItemDeselected(IVisualCanBeSelected deselected)
        {
            deselected.IsSelected = false;
            RemoveItemSelected(deselected);
        }
        public void ItemsSelectedFromSelectionBox(IEnumerable<IVisualCanBeSelected> selected)
        {
            var selectedHS = new HashSet<IVisualCanBeSelected>(selected);
            var toRemove = new List<IVisualCanBeSelected>();

            ClearItemsSelected();
            foreach (var item in selected)
            {
                AddItemSelected(item);
            }
        }
        public void ClearItemsSelected()
        {
            foreach (var visual in Chart.VisualItemsSelected)
            {
                visual.IsSelected = false;
            }
            foreach (var visual in Chart.ChartEntitiesSelected)
            {
                visual.IsSelected = false;
            }
            foreach (var visual in Chart.VisualTextsSelected)
            {
                visual.IsSelected = false;
            }
            Chart.VisualItemsSelected.Clear();
            Chart.ChartEntitiesSelected.Clear();
            Chart.VisualTextsSelected.Clear();
        }
        #endregion

        #region moving
        public HashSet<SelectedBorderMovingData> GetSelectedBordersMovingCurrentPosition(IVisualMoveRotate except)
        {
            var result = new HashSet<SelectedBorderMovingData>();

            foreach (var item in Chart.ChartEntitiesSelected)
            {
                var vm = item as IChartEntityViewModel;
                var border = vm.UserControl as ChartEntityMoveRotate;
                if (border == null)
                    continue;
                if (border.Equals(except))
                    continue;
                var data = new SelectedBorderMovingData()
                {
                    Visual = border,
                    MovingThumbPositionWhenClickedRelativeToCanvas = border.BaseMovingThumb.GetAnchorPoint(),
                    ItemCoordsWhenClickedRelativeToCanvas = new Point(Canvas.GetLeft(border), Canvas.GetTop(border))
                };

                result.Add(data);
            }
            foreach (var item in Chart.VisualTextsSelected)
            {
                var vm = item as IVisualTextViewModel;
                var border = vm.UserControl as ChartEntityMoveRotate;
                if (border == null)
                    continue;
                if (border.Equals(except))
                    continue;
                var data = new SelectedBorderMovingData()
                {
                    Visual = border,
                    MovingThumbPositionWhenClickedRelativeToCanvas = border.BaseMovingThumb.GetAnchorPoint(),
                    ItemCoordsWhenClickedRelativeToCanvas = new Point(Canvas.GetLeft(border), Canvas.GetTop(border))
                };

                result.Add(data);
            }

            return result;
        }
        public HashSet<SelectedBorderResizingData> GetSelectedBordersResizingCurrentSize(IVisualResizeMoveRotate except)
        {
            var result = new HashSet<SelectedBorderResizingData>();

            foreach (var item in Chart.ChartEntitiesSelected)
            {
                var vm = item as IChartEntityViewModel;
                var border = vm.UserControl as ChartEntityResizeWithViewBoxMoveRotate;
                if (border == null)
                    continue;
                if (border.Equals(except))
                    continue;

                var data = new SelectedBorderResizingData()
                {
                    Visual = border as IVisualResizeMoveRotate,
                    MovingThumbPositionWhenClickedRelativeToCanvas = border.BaseResizingThumb.GetAnchorPoint(),
                    OriginalItemSize = new Size(border.ActualWidth, border.ActualHeight)
                };

                result.Add(data);
            }
            foreach (var item in Chart.VisualTextsSelected)
            {
                var vm = item as IVisualTextViewModel;
                var border = vm.UserControl as ChartEntityResizeMoveRotate;
                if (border == null)
                    continue;
                if (border.Equals(except))
                    continue;

                var data = new SelectedBorderResizingData()
                {
                    Visual = border,
                    MovingThumbPositionWhenClickedRelativeToCanvas = border.BaseResizingThumb.GetAnchorPoint(),
                    OriginalItemSize = new Size(border.ActualWidth, border.ActualHeight)
                };

                result.Add(data);
            }

            return result;
        }
        public HashSet<SelectedBorderRotatingData> GetSelectedBordersRotatingCurrentAngle(IVisualMoveRotate except)
        {
            var result = new HashSet<SelectedBorderRotatingData>();

            foreach (var item in Chart.ChartEntitiesSelected)
            {
                var vm = item as IChartEntityViewModel;
                var border = vm.UserControl as ChartEntityMoveRotate;
                if (border == null)
                    continue;
                if (border.Equals(except))
                    continue;

                var data = new SelectedBorderRotatingData()
                {
                    Visual = border,
                    OriginalAngle = border._RotateTransform.Angle
                };

                result.Add(data);
            }
            foreach (var item in Chart.VisualTextsSelected)
            {
                var vm = item as IVisualTextViewModel;
                var border = vm.UserControl as ChartEntityMoveRotate;
                if (border == null)
                    continue;
                if (border.Equals(except))
                    continue;

                var data = new SelectedBorderRotatingData()
                {
                    Visual = border,
                    OriginalAngle = border._RotateTransform.Angle
                };

                result.Add(data);
            }

            return result;
        }
        #endregion
    }
}
