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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartCanvasNamespace.Lines
{
    /// <summary>
    /// Lógica de interacción para LineDivider.xaml
    /// </summary>
    public partial class LineDivider : UserControl, IChartThumb
    {
        public LineDivider(LineBetweenConnectingThumbs line)
        {
            InitializeComponent();
            _MyLine = line;
            Width = Size;
            Height = Size;
            Panel.SetZIndex(this, 1000);
        }

        private LineBetweenConnectingThumbs _MyLine;
        public static int Size { get { return 10; } }

        protected void OnClick(object sender, RoutedEventArgs e)
        {
            _MyLine.DivideLine();
            e.Handled = true;
        }
    }
}
