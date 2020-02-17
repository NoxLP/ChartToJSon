using ChartToJson.View;
using ChartToJson.View.Windows;
using ChartToJson.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoomAndPan;

namespace ChartToJson
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                RotatePopup.IsOpen = true;
                ResizePopup.IsOpen = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var forget = Task.Run(() => WPFHelpers.Log.Instance.WriteAsync($@"

************************

************************ Log Application ChartToJson ; {DateTime.Now.ToLongDateString()}

************************

"));

            ChartCanvasNamespace.ChartCustomControl.SetMainInstance(MyChartControl);
            //MyChartControl.Focus();
            //var b = new Binding() { Source = MyChartControl, Path = new PropertyPath(ChartCanvasNamespace.ChartCustomControl.SnapToOtherEntitiesProperty), Mode = BindingMode.TwoWay };
            //BindingOperations.SetBinding(ToggleSnap, ToggleButton.IsCheckedProperty, b);
            //b = new Binding() { Source = MyChartControl, Path = new PropertyPath(ChartCanvasNamespace.ChartCustomControl.SnapToConnectionAnchorPointsProperty), Mode = BindingMode.TwoWay };
            //BindingOperations.SetBinding(ToggleSnapAnchor, ToggleButton.IsCheckedProperty, b);
            //MyChartControl.ExpandContent();
            //NewOverviewWindow();
            ((MainVM)DataContext).OnWindowLoaded(this);
        }

        public OverviewWindow NewOverviewWindow()
        {
            OverviewWindow overviewWindow = new OverviewWindow();
            overviewWindow.Left = this.Left;
            overviewWindow.Top = this.Top + this.ActualHeight - overviewWindow.Height;
            overviewWindow.Owner = this;
            overviewWindow.Show();
            return overviewWindow;
        }

        #region undo/redo buttons
        private void UndoToggle_Checked(object sender, RoutedEventArgs e)
        {
            UndoPopup.IsOpen = true;
        }
        private void UndoToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if(UndoPopup.IsOpen)
                UndoPopup.IsOpen = false;
        }
        private void UndoPopup_Closed(object sender, EventArgs e)
        {
            UndoToggle.IsChecked = false;
        }
        private void RedoToggle_Checked(object sender, RoutedEventArgs e)
        {
            RedoPopup.IsOpen = true;
        }
        private void RedoToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (RedoPopup.IsOpen)
                RedoPopup.IsOpen = false;
        }
        private void RedoPopup_Closed(object sender, EventArgs e)
        {
            RedoToggle.IsChecked = false;
        }
        #endregion

        private void GridConfigButton_Click(object sender, RoutedEventArgs e)
        {
            GridConfigPopup.IsOpen = true;
        }

        private void ChangeSizeManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeCanvasSizePopup.IsOpen = true;
        }

        private void ChangeShapeButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeShapePopup.IsOpen = true;
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            RotatePopup.IsOpen = true;
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            ResizePopup.IsOpen = true;
        }

        private void FontChooserButton_Click(object sender, RoutedEventArgs e)
        {
            FontChooserPopup.IsOpen = true;
        }

        private void EntitiesColorButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeColorPopup.IsOpen = true;
        }
    }
}

#region old
/*
 * #region zoom and pan
        //https://www.codeproject.com/Articles/85603/A-WPF-custom-control-for-zooming-and-panning

        #region fields
        private double _ZoomSpeed = 0.01d;
        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode _MouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point _OrigZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point _OrigContentMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton _MouseButtonDown;

        /// <summary>
        /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
        /// </summary>
        private Rect _PrevZoomRect;

        /// <summary>
        /// Save the previous content scale, pressing the backspace key jumps back to this scale.
        /// </summary>
        private double _PrevZoomScale;

        /// <summary>
        /// Set to 'true' when the previous zoom rect is saved.
        /// </summary>
        private bool _PrevZoomRectSet = false;
        #endregion

        /// <summary>
        /// Expand the content area to fit the rectangles.
        /// </summary>
        private void ExpandContent()
        {
            //double xOffset = 0;
            //double yOffset = 0;
            //Rect contentRect = new Rect(0, 0, 0, 0);
            //foreach (RectangleData rectangleData in ZoomAndPanDataModel.Instance.Rectangles)
            //{
            //    if (rectangleData.X < xOffset)
            //    {
            //        xOffset = rectangleData.X;
            //    }

            //    if (rectangleData.Y < yOffset)
            //    {
            //        yOffset = rectangleData.Y;
            //    }

            //    contentRect.Union(new Rect(rectangleData.X, rectangleData.Y, rectangleData.Width, rectangleData.Height));
            //}

            ////
            //// Translate all rectangles so they are in positive space.
            ////
            //xOffset = Math.Abs(xOffset);
            //yOffset = Math.Abs(yOffset);

            //foreach (RectangleData rectangleData in ZoomAndPanDataModel.Instance.Rectangles)
            //{
            //    rectangleData.X += xOffset;
            //    rectangleData.Y += yOffset;
            //}

            //ZoomAndPanDataModel.Instance.ContentWidth = contentRect.Width;
            //ZoomAndPanDataModel.Instance.ContentHeight = contentRect.Height;
            ZoomAndPanDataModel.Instance.ContentWidth = RootGrid.ActualWidth;
            ZoomAndPanDataModel.Instance.ContentHeight = RootGrid.ActualHeight;
        }

        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //TV.Focus();
            //Keyboard.Focus(TV);
            MyChartControl.ClearItemsSelected();
            var mainVM = (MainVM)DataContext;
            if (!mainVM.AddingEmptyEntity)
            {
                _MouseButtonDown = e.ChangedButton;
                _OrigZoomAndPanControlMouseDownPoint = e.GetPosition(zoomAndPanControl);
                _OrigContentMouseDownPoint = e.GetPosition(MyChartControl);

                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                    (e.ChangedButton == MouseButton.Left ||
                     e.ChangedButton == MouseButton.Right))
                {
                    _MouseHandlingMode = MouseHandlingMode.Selecting;
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
                    (e.ChangedButton == MouseButton.Left ||
                     e.ChangedButton == MouseButton.Right))
                {
                    // Control + left- or right-down initiates zooming mode.
                    _MouseHandlingMode = MouseHandlingMode.Zooming;
                }
                else if (_MouseButtonDown == MouseButton.Middle)
                {
                    // Just a plain old left-down initiates panning mode.
                    _MouseHandlingMode = MouseHandlingMode.Panning;
                }

                if (_MouseHandlingMode != MouseHandlingMode.None)
                {
                    // Capture the mouse so that we eventually receive the mouse up event.
                    zoomAndPanControl.CaptureMouse();
                }
            }
        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var mainVM = (MainVM)DataContext;
            if (!mainVM.AddingEmptyEntity && _MouseHandlingMode != MouseHandlingMode.None)
            {
                if (_MouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (_MouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the content.
                        ZoomIn(_OrigZoomAndPanControlMouseDownPoint);
                    }
                    else if (_MouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the content.
                        ZoomOut(_OrigZoomAndPanControlMouseDownPoint);
                    }
                }
                else if (_MouseHandlingMode == MouseHandlingMode.DragZooming)
                {
                    // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.

                    //
                    // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
                    //
                    SavePrevZoomRect();

                    MyChartControl.ApplyDragZoomRect(zoomAndPanControl);
                }
                else if (_MouseHandlingMode == MouseHandlingMode.DragSelecting)
                {
                    MyChartControl.ApplyDragSelectionBox();
                }
                
                zoomAndPanControl.ReleaseMouseCapture();
                _MouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
            else if(mainVM.AddingEmptyEntity)
            {
                mainVM.MouseUpWhileAddingEntity(e.GetPosition(zoomAndPanControl));
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
        {
            ChartCanvasNamespace.ChartCustomControl.Instance.HideEntityThumbsIfAny();
            ChartCanvasNamespace.ChartCustomControl.Instance.HideLineThumbsIfAny();
            if (_MouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(MyChartControl);
                Vector dragOffset = curContentMousePoint - _OrigContentMouseDownPoint;

                zoomAndPanControl.ContentOffsetX -= dragOffset.X;
                zoomAndPanControl.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else if (_MouseHandlingMode == MouseHandlingMode.Zooming || _MouseHandlingMode == MouseHandlingMode.Selecting)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - _OrigZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (_MouseButtonDown == MouseButton.Left &&
                    (Math.Abs(dragOffset.X) > dragThreshold ||
                     Math.Abs(dragOffset.Y) > dragThreshold))
                {
                    //
                    // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                    // initiate drag zooming mode where the user can drag out a rectangle to select the area
                    // to zoom in on.
                    //
                    Point curContentMousePoint = e.GetPosition(MyChartControl);

                    if (_MouseHandlingMode == MouseHandlingMode.Zooming)
                    {
                        _MouseHandlingMode = MouseHandlingMode.DragZooming;
                        MyChartControl.InitDragZoomRect(_OrigContentMouseDownPoint, curZoomAndPanControlMousePoint);
                    }
                    else
                    {
                        _MouseHandlingMode = MouseHandlingMode.DragSelecting;
                        MyChartControl.InitDragSelectionBox(_OrigContentMouseDownPoint, curContentMousePoint);
                    }
                }

                e.Handled = true;
            }
            else if (_MouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                //
                // When in drag zooming mode continously update the position of the rectangle
                // that the user is dragging out.
                //
                Point curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
                MyChartControl.SetDragZoomRect(_OrigContentMouseDownPoint, curZoomAndPanControlMousePoint);

                e.Handled = true;
            }
            else if(_MouseHandlingMode == MouseHandlingMode.DragSelecting)
            {
                Point curContentMousePoint = e.GetPosition(MyChartControl);
                MyChartControl.SetDragSelectionBox(_OrigContentMouseDownPoint, curContentMousePoint);
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        private void zoomAndPanControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(zoomAndPanControl);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(zoomAndPanControl);
                ZoomOut(curContentMousePoint);
            }
        }

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'JumpBackToPrevZoom' command was executed.
        /// </summary>
        private void JumpBackToPrevZoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            JumpBackToPrevZoom();
        }

        /// <summary>
        /// Determines whether the 'JumpBackToPrevZoom' command can be executed.
        /// </summary>
        private void JumpBackToPrevZoom_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _PrevZoomRectSet;
        }

        /// <summary>
        /// The 'Fill' command was executed.
        /// </summary>
        private void Fill_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomAndPanControl.AnimatedScaleToFit();
        }

        /// <summary>
        /// The 'OneHundredPercent' command was executed.
        /// </summary>
        private void OneHundredPercent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomAndPanControl.AnimatedZoomTo(1.0);
        }

        /// <summary>
        /// Jump back to the previous zoom level.
        /// </summary>
        private void JumpBackToPrevZoom()
        {
            zoomAndPanControl.AnimatedZoomTo(_PrevZoomScale, _PrevZoomRect);

            ClearPrevZoomRect();
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale - _ZoomSpeed, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale + _ZoomSpeed, contentZoomCenter);
        }

        

        /// <summary>
        /// Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        /// </summary>
        private void SavePrevZoomRect()
        {
            _PrevZoomRect = new Rect(zoomAndPanControl.ContentOffsetX, zoomAndPanControl.ContentOffsetY, zoomAndPanControl.ContentViewportWidth, zoomAndPanControl.ContentViewportHeight);
            _PrevZoomScale = zoomAndPanControl.ContentScale;
            _PrevZoomRectSet = true;
        }

        /// <summary>
        /// Clear the memory of the previous zoom level.
        /// </summary>
        private void ClearPrevZoomRect()
        {
            _PrevZoomRectSet = false;
        }

        /// <summary>
        /// Event raised when the user has double clicked in the zoom and pan control.
        /// </summary>
        private void zoomAndPanControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                Point doubleClickPoint = e.GetPosition(MyChartControl);
                zoomAndPanControl.AnimatedSnapTo(doubleClickPoint);
            }
        }
        #endregion
 * */
#endregion
