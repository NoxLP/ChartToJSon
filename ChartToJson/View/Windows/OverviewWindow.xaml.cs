using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using ChartCanvasNamespace.ZoomAndPanModel;

namespace ChartToJson.View.Windows
{
    /// <summary>
    /// Interaction logic for OverviewWindow.xaml
    /// </summary>
    public partial class OverviewWindow : Window
    {
        public OverviewWindow()
        {
            InitializeComponent();
        }

        public bool IsClosed { get; private set; }

        /// <summary>
        /// Event raised when the size of the ZoomAndPanControl changes.
        /// </summary>
        private void overview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // Update the scale so that the entire content fits in the window.
            //
            overview.ScaleToFit();
        }

        /// <summary>
        /// Event raised when the user drags the overview zoom rect.
        /// </summary>
        private void overviewZoomRectThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //
            // Update the position of the overview rect as the user drags it around.
            //
            double newContentOffsetX = Math.Min(Math.Max(0.0, Canvas.GetLeft(overviewZoomRectThumb) + e.HorizontalChange), ZoomAndPanDataModel.Instance.ContentWidth - ZoomAndPanDataModel.Instance.ContentViewportWidth);
            Canvas.SetLeft(overviewZoomRectThumb, newContentOffsetX);

            double newContentOffsetY = Math.Min(Math.Max(0.0, Canvas.GetTop(overviewZoomRectThumb) + e.VerticalChange), ZoomAndPanDataModel.Instance.ContentHeight - ZoomAndPanDataModel.Instance.ContentViewportHeight);
            Canvas.SetTop(overviewZoomRectThumb, newContentOffsetY);
        }

        /// <summary>
        /// Event raised on mouse down.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // Update the position of the overview rect to the point that was clicked.
            //
            Point clickedPoint = e.GetPosition(content);
            double newX = clickedPoint.X - (overviewZoomRectThumb.Width / 2);
            double newY = clickedPoint.Y - (overviewZoomRectThumb.Height / 2);
            Canvas.SetLeft(overviewZoomRectThumb, newX);
            Canvas.SetTop(overviewZoomRectThumb, newY);
        }

        #region hide close button
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwndSource.AddHook(HwndSourceHook);
            }

        }

        public bool _AllowClosing = false;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;

        private const uint SC_CLOSE = 0xF060;

        private const int WM_SHOWWINDOW = 0x00000018;
        private const int WM_CLOSE = 0x10;

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SHOWWINDOW:
                    {
                        IntPtr hMenu = GetSystemMenu(hwnd, false);
                        if (hMenu != IntPtr.Zero)
                        {
                            EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                        }
                    }
                    break;
                case WM_CLOSE:
                    if (!_AllowClosing)
                    {
                        handled = true;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}
