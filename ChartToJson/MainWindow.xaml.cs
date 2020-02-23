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
