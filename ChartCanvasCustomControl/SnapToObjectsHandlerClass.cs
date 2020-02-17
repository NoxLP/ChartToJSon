//#define DEBUG_SNAP_LINES

using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Lines;
using ChartCanvasNamespace.OtherVisuals;
using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFHelpers;

namespace ChartCanvasNamespace
{
    public class SnapToObjectsHandlerClass
    {
        public SnapToObjectsHandlerClass()
        {
            SnapYCoordinatesEntities = new Dictionary<double, List<IVisualWithSnappingCoordinates>>();
            SnapXCoordinatesEntities = new Dictionary<double, List<IVisualWithSnappingCoordinates>>();
            SnapYCoordinatesLineAnchorPoints = new Dictionary<double, List<IVisualWithSnappingCoordinates>>();
            SnapXCoordinatesLineAnchorPoints = new Dictionary<double, List<IVisualWithSnappingCoordinates>>();
            SnapXCoordinatesLineDraggers = new Dictionary<double, List<LineDragger>>();
            SnapYCoordinatesLineDraggers = new Dictionary<double, List<LineDragger>>();
        }

        public enum SnapBorderTypeEnum { None, Center, Left, Right, Top, Bottom, ThLeft, ThRight, ThTop, ThBottom }
        public class SnapTo
        {
            public double? X;
            public double? Y;
            public SnapBorderTypeEnum XType = SnapBorderTypeEnum.None;
            public SnapBorderTypeEnum YType = SnapBorderTypeEnum.None;
            public bool Snap { get { return X.HasValue || Y.HasValue; } }

            public override string ToString()
            {
                return $"({(X.HasValue ? X.ToString() : "null")}, {(Y.HasValue ? Y.ToString() : "null")}";
            }
        }

        #region fields
        private IVisualWithSnappingCoordinates _LastUCSnapped;
        private SnapTo _LastSnap;
        private TemporalCurrentSnapCoordinates _LastCoords;
        private double _LastMinDistanceX;
        private double _LastMinDistanceY;
        #endregion

        #region properties
        public Dictionary<double, List<IVisualWithSnappingCoordinates>> SnapYCoordinatesEntities { get; private set; }
        public Dictionary<double, List<IVisualWithSnappingCoordinates>> SnapXCoordinatesEntities { get; private set; }
        public Dictionary<double, List<IVisualWithSnappingCoordinates>> SnapYCoordinatesLineAnchorPoints { get; private set; }
        public Dictionary<double, List<IVisualWithSnappingCoordinates>> SnapXCoordinatesLineAnchorPoints { get; private set; }
        public Dictionary<double, List<LineDragger>> SnapYCoordinatesLineDraggers { get; private set; }
        public Dictionary<double, List<LineDragger>> SnapXCoordinatesLineDraggers { get; private set; }
        #endregion

        #region check snapping
        private bool BorderCanSnapToBorder(IVisualWithSnappingCoordinates movingBorder, List<IVisualWithSnappingCoordinates> storedBorders)
        {
            if (movingBorder == null)
                return true;

            if (storedBorders.Count <= 1 && storedBorders.Contains(movingBorder))
                return false;

            var selectedItems = ChartCustomControl.Instance.ChartEntitiesSelected;
            if (selectedItems.Count > 1)
            {
                if (storedBorders.Any(x => !selectedItems.Contains((IChartEntityViewModel)((UserControl)x).DataContext)))
                    return true;
                return false;
            }

            return true;
        }
        private bool DraggerCanSnapToDragger(LineDragger moving, List<LineDragger> stored)
        {
            if (moving == null)
                return true;

            if (stored.Count <= 1 && stored.Contains(moving))
                return false;

            return true;
        }
        private void CheckSnapXBorderToGrid(bool first, IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            double range = Properties.Settings.Default.ThumbsSnapRange;
            _LastMinDistanceX = range;
            double gridWidth = ChartCustomControl.Instance.GridLength;

            double gridX = Math.Round(coords.CenterX / gridWidth) * gridWidth;
            var distance = Math.Abs(coords.CenterX - gridX);
            if(distance < _LastMinDistanceX)
            {
                _LastMinDistanceX = distance;
                _LastSnap.X = gridX;
                _LastSnap.XType = SnapBorderTypeEnum.Center;
            }
            gridX = Math.Round(coords.Left / gridWidth) * gridWidth;
            distance = Math.Abs(coords.Left - gridX);
            if (distance < _LastMinDistanceX)
            {
                _LastMinDistanceX = distance;
                _LastSnap.X = gridX;
                _LastSnap.XType = SnapBorderTypeEnum.Left;
            }
            gridX = Math.Round(coords.Right / gridWidth) * gridWidth;
            distance = Math.Abs(coords.Right - gridX);
            if (distance < _LastMinDistanceX)
            {
                _LastMinDistanceX = distance;
                _LastSnap.X = gridX;
                _LastSnap.XType = SnapBorderTypeEnum.Right;
            }
        }
        private void CheckSnapYBorderToGrid(bool first, IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            double range = Properties.Settings.Default.ThumbsSnapRange;
            _LastMinDistanceY = range;
            double gridWidth = ChartCustomControl.Instance.GridLength;

            double gridY = Math.Round(coords.CenterY / gridWidth) * gridWidth;
            var distance = Math.Abs(coords.CenterY - gridY);
            if (distance < _LastMinDistanceY)
            {
                _LastMinDistanceY = distance;
                _LastSnap.Y = gridY;
                _LastSnap.YType = SnapBorderTypeEnum.Center;
            }
            gridY = Math.Round(coords.Left / gridWidth) * gridWidth;
            distance = Math.Abs(coords.Left - gridY);
            if (distance < _LastMinDistanceY)
            {
                _LastMinDistanceY = distance;
                _LastSnap.Y = gridY;
                _LastSnap.YType = SnapBorderTypeEnum.Left;
            }
            gridY = Math.Round(coords.Right / gridWidth) * gridWidth;
            distance = Math.Abs(coords.Right - gridY);
            if (distance < _LastMinDistanceY)
            {
                _LastMinDistanceY = distance;
                _LastSnap.Y = gridY;
                _LastSnap.YType = SnapBorderTypeEnum.Right;
            }
        }
        private void CheckSnapXBorderToBorders(bool first, IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            if (first)
            {
                double range = Properties.Settings.Default.ThumbsSnapRange;
                _LastMinDistanceX = range;
            }

            foreach (var kvp in SnapXCoordinatesEntities)
            {
                if (BorderCanSnapToBorder(uc, kvp.Value))//uc != null && (!kvp.Value.Contains(uc) || kvp.Value.Count > 1))
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key}"));
#endif
                    var distance = Math.Abs(coords.CenterX - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to center X with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                        _LastSnap.XType = SnapBorderTypeEnum.Center;
                    }

                    distance = Math.Abs(coords.Left - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to left with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                        _LastSnap.XType = SnapBorderTypeEnum.Left;
                    }

                    distance = Math.Abs(coords.Right - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to right with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                        _LastSnap.XType = SnapBorderTypeEnum.Right;
                    }
                }
            }
        }
        private void CheckSnapYBorderToBorders(bool first, IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            if (first)
            {
                double range = Properties.Settings.Default.ThumbsSnapRange;
                _LastMinDistanceY = range;
            }
            foreach (var kvp in SnapYCoordinatesEntities)
            {
                if (BorderCanSnapToBorder(uc, kvp.Value))//uc != null && (!kvp.Value.Contains(uc) || kvp.Value.Count > 1))
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key}"));
#endif
                    var distance = Math.Abs(coords.CenterY - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to center Y with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                        _LastSnap.YType = SnapBorderTypeEnum.Center;
                    }

                    distance = Math.Abs(coords.Top - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to top with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                        _LastSnap.YType = SnapBorderTypeEnum.Top;
                    }

                    distance = Math.Abs(coords.Bottom - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to bottom with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                        _LastSnap.YType = SnapBorderTypeEnum.Bottom;
                    }
                }
            }
        }
        private void CheckSnapXBorderToLineAnchorPoints(IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            foreach (var kvp in SnapXCoordinatesLineAnchorPoints)
            {
                if (BorderCanSnapToBorder(uc, kvp.Value))//uc != null && (!kvp.Value.Contains(uc) || kvp.Value.Count > 1))
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key}"));
#endif
                    var distance = Math.Abs(coords.CenterX - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to center X with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                        _LastSnap.XType = SnapBorderTypeEnum.Center;
                    }

                    distance = Math.Abs(coords.ThLeft - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to left with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                        _LastSnap.XType = SnapBorderTypeEnum.ThLeft;
                    }

                    distance = Math.Abs(coords.ThRight - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to right with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                        _LastSnap.XType = SnapBorderTypeEnum.ThRight;
                    }
                }
            }
        }
        private void CheckSnapYBorderToLineAnchorPoints(IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            foreach (var kvp in SnapYCoordinatesLineAnchorPoints)
            {
                if (BorderCanSnapToBorder(uc, kvp.Value))//uc != null && (!kvp.Value.Contains(uc) || kvp.Value.Count > 1))
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key}"));
#endif
                    var distance = Math.Abs(coords.CenterY - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to center Y with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                        _LastSnap.YType = SnapBorderTypeEnum.Center;
                    }

                    distance = Math.Abs(coords.ThTop - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to top with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                        _LastSnap.YType = SnapBorderTypeEnum.ThTop;
                    }

                    distance = Math.Abs(coords.ThBottom - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to bottom with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                        _LastSnap.YType = SnapBorderTypeEnum.ThBottom;
                    }
                }
            }
        }

        private void CheckSnapXLineDraggerToGrid(Point point, LineDragger dragger)
        {
            double range = Properties.Settings.Default.ThumbsSnapRange;
            _LastMinDistanceX = range;
            double gridWidth = ChartCustomControl.Instance.GridLength;

            double gridX = Math.Round(point.X / gridWidth) * gridWidth;
            var distance = Math.Abs(point.X - gridX);
            if (distance < _LastMinDistanceX)
            {
                _LastMinDistanceX = distance;
                _LastSnap.X = gridX;
            }
        }
        private void CheckSnapYLineDraggerToGrid(Point point, LineDragger dragger)
        {
            double range = Properties.Settings.Default.ThumbsSnapRange;
            _LastMinDistanceY = range;
            double gridWidth = ChartCustomControl.Instance.GridLength;

            double gridY = Math.Round(point.Y / gridWidth) * gridWidth;
            var distance = Math.Abs(point.Y - gridY);
            if (distance < _LastMinDistanceY)
            {
                _LastMinDistanceY = distance;
                _LastSnap.Y = gridY;
            }
        }
        private void CheckSnapXLineDraggerToBorders(bool first, Point point, LineDragger dragger)
        {
            if (first)
                _LastMinDistanceX = Properties.Settings.Default.ThumbsSnapRange;
            double distance;
            foreach (var kvp in SnapXCoordinatesEntities)
            {
                distance = Math.Abs(point.X - kvp.Key);
                if (distance < _LastMinDistanceX)
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to point with distance {distance}"));
#endif
                    _LastMinDistanceX = distance;
                    _LastSnap.X = kvp.Key;
                }
            }
            foreach (var kvp in SnapXCoordinatesLineDraggers)
            {
                if (!DraggerCanSnapToDragger(dragger, kvp.Value))
                    continue;

                distance = Math.Abs(point.X - kvp.Key);
                if (distance < _LastMinDistanceX)
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapXCoords: {kvp.Key} to point with distance {distance}"));
#endif
                    _LastMinDistanceX = distance;
                    _LastSnap.X = kvp.Key;
                }
            }
            if (ChartCustomControl.Instance.SnapToConnectionAnchorPoints)
            {
                foreach (var kvp in SnapXCoordinatesLineAnchorPoints)
                {
                    distance = Math.Abs(point.X - kvp.Key);
                    if (distance < _LastMinDistanceX)
                    {
#if DEBUG_SNAP_LINES
                        var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to point with distance {distance}"));
#endif
                        _LastMinDistanceX = distance;
                        _LastSnap.X = kvp.Key;
                    }
                }
            }
        }
        private void CheckSnapYLineDraggerToBorders(bool first, Point point, LineDragger dragger)
        {
            if (first)
                _LastMinDistanceY = Properties.Settings.Default.ThumbsSnapRange;
            double distance;
            foreach (var kvp in SnapYCoordinatesEntities)
            {
                distance = Math.Abs(point.Y - kvp.Key);
                if (distance < _LastMinDistanceY)
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to point with distance {distance}"));
#endif
                    _LastMinDistanceY = distance;
                    _LastSnap.Y = kvp.Key;
                }
            }
            foreach (var kvp in SnapYCoordinatesLineDraggers)
            {
                if (!DraggerCanSnapToDragger(dragger, kvp.Value))
                    continue;

                distance = Math.Abs(point.Y - kvp.Key);
                if (distance < _LastMinDistanceY)
                {
#if DEBUG_SNAP_LINES
                    var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to point with distance {distance}"));
#endif
                    _LastMinDistanceY = distance;
                    _LastSnap.Y = kvp.Key;
                }
            }
            if (ChartCustomControl.Instance.SnapToConnectionAnchorPoints)
            {
                foreach (var kvp in SnapYCoordinatesLineAnchorPoints)
                {
                    distance = Math.Abs(point.Y - kvp.Key);
                    if (distance < _LastMinDistanceY)
                    {
#if DEBUG_SNAP_LINES
                        var forget = Task.Run(() => Log.Instance.WriteAsync($"    - snapYCoords: {kvp.Key} to point with distance {distance}"));
#endif
                        _LastMinDistanceY = distance;
                        _LastSnap.Y = kvp.Key;
                    }
                }
            }
        }

        public SnapTo CheckPointShouldSnap(IVisualWithSnappingCoordinates uc, TemporalCurrentSnapCoordinates coords)
        {
            if (!ChartCustomControl.Instance.SnapToGrid && !ChartCustomControl.Instance.SnapToOtherEntities)
                return null;

            if (coords.EqualsTruncated(_LastCoords))
                return _LastSnap;

#if DEBUG_SNAP_LINES
            Task forget;
#endif

            _LastSnap = new SnapTo();

#if DEBUG_SNAP_LINES
            forget = Task.Run(() => Log.Instance.WriteAsync($@"check snap:
coords: {coords.ToString()}"));
//range: {range}"));
#endif

            if(ChartCustomControl.Instance.SnapToGrid)
            {
                CheckSnapXBorderToGrid(true, uc, coords);
                CheckSnapYBorderToGrid(true, uc, coords);
            }

            if (ChartCustomControl.Instance.SnapToOtherEntities)
            {
                CheckSnapXBorderToBorders(!ChartCustomControl.Instance.SnapToGrid, uc, coords);
                CheckSnapYBorderToBorders(!ChartCustomControl.Instance.SnapToGrid, uc, coords);

                if (ChartCustomControl.Instance.SnapToConnectionAnchorPoints)
                {
                    CheckSnapXBorderToLineAnchorPoints(uc, coords);
                    CheckSnapYBorderToLineAnchorPoints(uc, coords);
                }
            }

            if (!_LastSnap.Snap)
            {
                HidSnapLines();
                return null;
            }

            if (_LastSnap.X.HasValue)
                ShowVerticalLine(_LastSnap.X.Value);
            if (_LastSnap.Y.HasValue)
                ShowHorizontalLine(_LastSnap.Y.Value);

            _LastUCSnapped = uc;
            return _LastSnap;
        }
        public SnapTo CheckLineDraggerPointShouldSnap(Point point, LineDragger dragger)
        {
            if (!ChartCustomControl.Instance.SnapToGrid && !ChartCustomControl.Instance.SnapToOtherEntities)
                return null;

#if DEBUG_SNAP_LINES
            Task forget;
#endif

            _LastSnap = new SnapTo();
            double toCheck = point.Y;


#if DEBUG_SNAP_LINES
            forget = Task.Run(() => Log.Instance.WriteAsync($@"check snap:
point: {point.ToString()}"));
//range: {range}"));
#endif

            if (ChartCustomControl.Instance.SnapToGrid)
            {
                CheckSnapXLineDraggerToGrid(point, dragger);
                CheckSnapYLineDraggerToGrid(point, dragger);
            }
            if(ChartCustomControl.Instance.SnapToOtherEntities)
            {
                CheckSnapXLineDraggerToBorders(!ChartCustomControl.Instance.SnapToGrid, point, dragger);
                CheckSnapYLineDraggerToBorders(!ChartCustomControl.Instance.SnapToGrid, point, dragger);
            }

            if (!_LastSnap.Snap)
            {
                HidSnapLines();
                return null;
            }

            if (_LastSnap.X.HasValue)
                ShowHorizontalLine(_LastSnap.X.Value);
            if (_LastSnap.Y.HasValue)
                ShowVerticalLine(_LastSnap.Y.Value);

            _LastUCSnapped = null;
            return _LastSnap;
        }
        #endregion

        #region update snap coordinates
        public void UpdateSnapAddBorder(IVisualWithSnappingCoordinates border)
        {
            SnapXCoordinatesEntities.AddListOrItemToDictionary(border.CenterX, border);
            var forget = Task.Run(() => Log.Instance.WriteAsync("Diccionario snap X"));
            SnapXCoordinatesEntities.AddListOrItemToDictionary(border.Left.X, border);
            SnapXCoordinatesEntities.AddListOrItemToDictionary(border.Right.X, border);

            forget = Task.Run(() => Log.Instance.WriteAsync("Diccionario snap Y"));
            SnapYCoordinatesEntities.AddListOrItemToDictionary(border.CenterY, border);
            SnapYCoordinatesEntities.AddListOrItemToDictionary(border.Top.Y, border);
            SnapYCoordinatesEntities.AddListOrItemToDictionary(border.Bottom.Y, border);

            UpdateSnapAddLineAnchorPoints(border);

#if DEBUG_SNAP_LINES
            DrawHorizontalLine(border, border.CenterX);
            DrawVerticalLine(border, border.CenterY);
            DrawHorizontalLine(border, border.Right.X);
            DrawHorizontalLine(border, border.Left.X);
            DrawVerticalLine(border, border.Top.Y);
            DrawVerticalLine(border, border.Bottom.Y);
#endif
        }
        public void UpdateSnapAddLineDragger(LineDragger dragger)
        {
            SnapXCoordinatesLineDraggers.AddListOrItemToDictionary(dragger.AnchorPoint.X, dragger);
            SnapYCoordinatesLineDraggers.AddListOrItemToDictionary(dragger.AnchorPoint.Y, dragger);

#if DEBUG_SNAP_LINES
            DrawHorizontalLine(dragger, dragger.AnchorPoint.X);
            DrawVerticalLine(dragger, dragger.AnchorPoint.Y);
#endif
        }
        public void UpdateSnapRemoveBorder(IChartEntityViewModel entityVM)
        {
            var border = (IVisualWithSnappingCoordinates)entityVM.UserControl;

            //Remove snap coordinates
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.CenterX, border);
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.Left.X, border);
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.Right.X, border);

            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.CenterY, border);
            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.Top.Y, border);
            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.Bottom.Y, border);

            UpdateSnapRemoveLineAnchorPoints(border);

#if DEBUG_SNAP_LINES
            RemoveHorizontalLines(border);
            RemoveVerticalLines(border);
#endif
        }
        public void UpdateSnapRemoveBorder(IVisualTextViewModel entityVM)
        {
            var border = (IVisualWithSnappingCoordinates)entityVM.UserControl;

            //Remove snap coordinates
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.CenterX, border);
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.Left.X, border);
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.Right.X, border);

            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.CenterY, border);
            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.Top.Y, border);
            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.Bottom.Y, border);

            UpdateSnapRemoveLineAnchorPoints(border);

#if DEBUG_SNAP_LINES
            RemoveHorizontalLines(border);
            RemoveVerticalLines(border);
#endif
        }
        internal void UpdateSnapRemoveBorder(IVisualWithSnappingCoordinates border)
        {
            //Remove snap coordinates
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.CenterX, border);
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.Left.X, border);
            SnapXCoordinatesEntities.RemoveListOrItemFromDictionary(border.Right.X, border);

            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.CenterY, border);
            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.Top.Y, border);
            SnapYCoordinatesEntities.RemoveListOrItemFromDictionary(border.Bottom.Y, border);

            UpdateSnapRemoveLineAnchorPoints(border);

#if DEBUG_SNAP_LINES
            RemoveHorizontalLines(border);
            RemoveVerticalLines(border);
#endif
        }
        internal void UpdateSnapRemoveLineDragger(Point old, LineDragger dragger)
        {
            //Remove snap coordinates
            SnapXCoordinatesLineDraggers.RemoveListOrItemFromDictionary(old.X, dragger);
            SnapYCoordinatesLineDraggers.RemoveListOrItemFromDictionary(old.Y, dragger);

#if DEBUG_SNAP_LINES
            RemoveHorizontalLines(dragger);
            RemoveVerticalLines(dragger);
#endif
        }
        private void UpdateSnapAddLineAnchorPoints(IVisualWithSnappingCoordinates visual)
        {
            //var margin = Properties.Settings.Default.LinesSnapMargin;
            var border = (IVisualWithConnectingThumbs)visual;
            SnapXCoordinatesLineAnchorPoints.AddListOrItemToDictionary(visual.CenterX, visual);
            SnapXCoordinatesLineAnchorPoints.AddListOrItemToDictionary(border._ThLeft.AnchorPoint.X, visual);
            SnapXCoordinatesLineAnchorPoints.AddListOrItemToDictionary(border._ThRight.AnchorPoint.X, visual);

            SnapYCoordinatesLineAnchorPoints.AddListOrItemToDictionary(visual.CenterY, visual);
            SnapYCoordinatesLineAnchorPoints.AddListOrItemToDictionary(border._ThTop.AnchorPoint.Y, visual);
            SnapYCoordinatesLineAnchorPoints.AddListOrItemToDictionary(border._ThBottom.AnchorPoint.Y, visual);

#if DEBUG_SNAP_LINES
            DrawHorizontalLine(border, border.ThLeft.AnchorPoint.X, Brushes.Red);
            DrawHorizontalLine(border, border.ThRight.AnchorPoint.X, Brushes.Red);
            DrawVerticalLine(border, border.ThTop.AnchorPoint.Y, Brushes.Red);
            DrawVerticalLine(border, border.ThBottom.AnchorPoint.Y, Brushes.Red);
#endif
        }
        private void UpdateSnapRemoveLineAnchorPoints(IVisualWithSnappingCoordinates visual)
        {
            //var margin = Properties.Settings.Default.LinesSnapMargin;
            var border = (IVisualWithConnectingThumbs)visual;
            SnapXCoordinatesLineAnchorPoints.RemoveListOrItemFromDictionary(visual.CenterX, visual);
            SnapXCoordinatesLineAnchorPoints.RemoveListOrItemFromDictionary(border._ThLeft.AnchorPoint.X, visual);
            SnapXCoordinatesLineAnchorPoints.RemoveListOrItemFromDictionary(border._ThRight.AnchorPoint.X, visual);

            SnapYCoordinatesLineAnchorPoints.RemoveListOrItemFromDictionary(visual.CenterY, visual);
            SnapYCoordinatesLineAnchorPoints.RemoveListOrItemFromDictionary(border._ThTop.AnchorPoint.Y, visual);
            SnapYCoordinatesLineAnchorPoints.RemoveListOrItemFromDictionary(border._ThBottom.AnchorPoint.Y, visual);

#if DEBUG_SNAP_LINES
            RemoveHorizontalLines(border);
            RemoveVerticalLines(border);
#endif
        }
        #endregion

        #region helpers
        private void ShowHorizontalLine(double coord)
        {
            var line = ChartCustomControl.Instance.HorizontalSnapLine;
            Canvas.SetTop(line, coord);
            line.Visibility = Visibility.Visible;
        }
        private void ShowVerticalLine(double coord)
        {
            var line = ChartCustomControl.Instance.VerticalSnapLine;
            Canvas.SetLeft(line, coord);
            line.Visibility = Visibility.Visible;
        }
        public void HidSnapLines()
        {
            var line = ChartCustomControl.Instance.HorizontalSnapLine;
            if (line.Visibility != Visibility.Hidden)
                line.Visibility = Visibility.Hidden;
            line = ChartCustomControl.Instance.VerticalSnapLine;
            if (line.Visibility != Visibility.Hidden)
                line.Visibility = Visibility.Hidden;
        }
        #endregion

#if DEBUG_SNAP_LINES
        #region debug snap lines
        private Dictionary<object, List<Line>> _HorizontalLines = new Dictionary<object, List<Line>>();
        private Dictionary<object, List<Line>> _VerticalLines = new Dictionary<object, List<Line>>();

        private void DrawHorizontalLine(object source, double x, Brush brush = null)
        {
            if (_HorizontalLines.ContainsKey(x))
                return;
            var line = new Line();
            line.X1 = x;
            line.X2 = x;
            line.Y1 = -2000;
            line.Y2 = 2000;
            line.Stroke = brush == null ? Brushes.Blue : brush;
            line.StrokeThickness = 1d;
            ChartCustomControl.Instance.ChartCanvas.Children.Add(line);
            //Canvas.SetLeft(line, x);
            _HorizontalLines.AddListOrItemToDictionary(source, line);
        }
        private void DrawVerticalLine(object source, double y, Brush brush = null)
        {
            if (_VerticalLines.ContainsKey(y))
                return;

            var line = new Line();
            line.Y1 = y;
            line.Y2 = y;
            line.X1 = -2000;
            line.X2 = 2000;
            line.Stroke = brush == null ? Brushes.Blue : brush;
            line.StrokeThickness = 1d;
            ChartCustomControl.Instance.ChartCanvas.Children.Add(line);
            //Canvas.SetTop(line, y);
            _VerticalLines.AddListOrItemToDictionary(source, line);
        }
        private void RemoveHorizontalLines(object source)
        {
            if (!_HorizontalLines.ContainsKey(source))
                return;
            foreach (var item in _HorizontalLines[source])
            {
                ChartCustomControl.Instance.ChartCanvas.Children.Remove(item);
            }
            
            _HorizontalLines.Remove(source);
        }
        private void RemoveVerticalLines(object source)
        {
            if (!_VerticalLines.ContainsKey(source))
                return;
            foreach (var item in _VerticalLines[source])
            {
                ChartCustomControl.Instance.ChartCanvas.Children.Remove(item);
            }
            
            _VerticalLines.Remove(source);
        }
        #endregion
#endif
    }
}
