using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChartToJson.View.Windows
{
    /// <summary>
    /// Lógica de interacción para ExportToImageConfigWindow.xaml
    /// </summary>
    public partial class ExportToImageConfigWindow : Window
    {
        public ExportToImageConfigWindow()
        {
            InitializeComponent();
        }

        private readonly double _ZoomSpeed = 0.01d;
        private double _Scale = 1;
        private Point _TransformOnClick;
        private Point _PointOnClick;
        private ScaleTransform _ScaleTransform;
        private TranslateTransform _TranslateTransform;
        private TransformGroup _TransformGroup;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel.MainVM)DataContext).OnExportWindowLoaded(this);

            _TranslateTransform = new TranslateTransform();
            _ScaleTransform = new ScaleTransform();
            _TransformGroup = new TransformGroup();
            _TransformGroup.Children.Add(_ScaleTransform);
            _TransformGroup.Children.Add(_TranslateTransform);

            ImagePreview.RenderTransform = _TransformGroup;
            ImagePreview.RenderTransformOrigin = new Point(0.5d, 0.5d);
        }
        private void IntegralInputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ((ViewModel.MainVM)DataContext).OnEnterKey();
                label.Focus();
            }
        }
        
        private void ImagePreview_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && _ScaleTransform.ScaleX <= _ZoomSpeed)
                return;

            double zoomNow = ImagePreview.RenderTransform.Value.M11;
            double valZoom = e.Delta > 0 ? _ZoomSpeed : -_ZoomSpeed;
            //Zoom(mousePosition, zoomNow + valZoom);
            _Scale = zoomNow + valZoom;
            _ScaleTransform.ScaleX = _Scale;
            _ScaleTransform.ScaleY = _Scale;
        }
        private void ImagePreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview.CaptureMouse();
            _PointOnClick = e.GetPosition(ImagePreview);
            _TransformOnClick = new Point(_TranslateTransform.X, _TranslateTransform.Y);
        }
        private void ImagePreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ImagePreview.IsMouseCaptured)
                return;

            Point pointOnMove = e.GetPosition(ImagePreview);

            if (double.IsNaN(pointOnMove.X) || double.IsNaN(pointOnMove.Y))
                return;

            _TranslateTransform.X = ImagePreview.RenderTransform.Value.OffsetX - ((_PointOnClick.X - pointOnMove.X) * _Scale);
            _TranslateTransform.Y = ImagePreview.RenderTransform.Value.OffsetY - ((_PointOnClick.Y - pointOnMove.Y) * _Scale);
            //ImagePreview.RenderTransformOrigin = pointOnMove;

            _PointOnClick = e.GetPosition(ImagePreview);
        }
        private void ImagePreview_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImagePreview.ReleaseMouseCapture();
        }
    }
}
