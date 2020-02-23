using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Selection;
using ChartCanvasNamespace.Thumbs;
using ChartCanvasNamespace.VisualsBase;
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
using UndoRedoSystem;

namespace ChartCanvasNamespace.OtherVisuals
{
    /// <summary>
    /// Lógica de interacción para TextUserControl.xaml
    /// </summary>
    public partial class TextUserControl :
        ChartEntityResizeMoveRotate, IChartObjectCanBeRemoved, IVisualWithConnectingThumbs, IVisualText
    {
        public TextUserControl()
        {
            InitializeComponent();
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_EntityBorder);

            _RotateTransform = new RotateTransform();
            RenderTransform = _RotateTransform;
            RenderTransformOrigin = new Point(0.5, 0.5);

            if (_SelectedBrush == null)
            {
                lock (_LockObject)
                {
                    if (_SelectedBrush == null)
                    {
                        _SelectedBrush = (SolidColorBrush)FindResource("SelectionBorderSelectedBrush");
                        _UnselectedBrush = (SolidColorBrush)FindResource("SelectionBorderTextUnSelectedBrush");
                    }
                }
            }
        }

        #region fields
        private static object _LockObject = new object();
        private bool _MovingThumbVisible = true;
        #endregion

        #region properties
        protected override double MinSize => 20;
        public override CanBeSelectedItemTypeEnum Type => CanBeSelectedItemTypeEnum.TextWithShape;
        public Point AnchorPoint { get; private set; }
        public override double CenterX { get; protected set; }
        public override double CenterY { get; protected set; }
        public EntityConnectingThumb _ThLeft => ThLeft;
        public EntityConnectingThumb _ThRight => ThRight;
        public EntityConnectingThumb _ThTop => ThTop;
        public EntityConnectingThumb _ThBottom => ThBottom;
        public override Grid BaseRootGrid => RootGrid;
        public override FrameworkElement ResizingControl => BorderContent;
        public override EntityMovingThumb BaseMovingThumb => MovingThumb;
        public override EntityResizingThumb BaseResizingThumb => ResizingThumb;
        public override EntityRotatingThumb BaseRotatingThumb => RotatingThumb;
        public double BorderContentWidth => BorderContent.Width;
        public double BorderContentHeight => BorderContent.Height;
        #endregion

        #region dependency properties
        public SolidColorBrush SelectionBorderBrush
        {
            get { return (SolidColorBrush)GetValue(SelectionBorderBrushProperty); }
            set { SetValue(SelectionBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty SelectionBorderBrushProperty =
            DependencyProperty.Register("SelectionBorderBrush", typeof(SolidColorBrush), typeof(TextUserControl), new PropertyMetadata(Brushes.Black));
        #endregion

        #region other events
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (IVisualTextViewModel)DataContext;
            vm.TextLoaded(this);
            Canvas.SetLeft(this, vm.CanvasX);// - AnchorPoint.X);
            Canvas.SetTop(this, vm.CanvasY);// - AnchorPoint.Y);
            var b = new Binding("Width") { Source = vm, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(BorderContent, Viewbox.WidthProperty, b);
            b = new Binding("Height") { Source = vm, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(BorderContent, Viewbox.HeightProperty, b);
            b = new Binding("Angle") { Source = vm, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(_RotateTransform, RotateTransform.AngleProperty, b);

            var uc = BorderContent;
            if (vm.Width == 0)
            {
                BorderContent.Width = uc.ActualWidth;
                BorderContent.Height = uc.ActualHeight;
            }
            UpdateAnchorPoint();
            DoActionAllConnectingThumbs(x => x.InitAnchorPoint());

            CalculateSnapCoords();
            ((IChartMainVM)ChartCustomControl.Instance.DataContext).CheckForPastedConnections(vm);
        }
        private void RootGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            ChartCustomControl.Instance._MouseEnterOnVisual++;
            if (!IsSelected)
                SelectionBorderBrush = _UnselectedBrush;
            ShowAllThumbs();
            Application.Current.MainWindow.KeyDown += KeyDown;
            Application.Current.MainWindow.KeyUp += KeyUp;
        }
        private void RootGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            ChartCustomControl.Instance._MouseEnterOnVisual--;
            if (!IsSelected)
                SelectionBorderBrush = Brushes.Transparent;
            HideAllThumbs();
            Application.Current.MainWindow.KeyDown -= KeyDown;
            Application.Current.MainWindow.KeyUp -= KeyUp;
            if (!_MovingThumbVisible)
            {
                _MovingThumbVisible = true;
                ToggleVisibleThumbs();
            }
        }
        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (_IsMoving || _IsResizing || _IsRotating)
                return;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _MovingThumbVisible = false;
                ToggleVisibleThumbs();
            }
        }
        private void KeyUp(object sender, KeyEventArgs e)
        {
            if (_IsMoving || _IsResizing || _IsRotating)
                return;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _MovingThumbVisible = true;
                ToggleVisibleThumbs();
            }
        }
        //private void MovingThumb_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (_IsMoving || _IsMoving || ResizingThumb.Visibility != Visibility.Visible)
        //        return;
        //    if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        //    {
        //        MovingThumb.Visibility = Visibility.Visible;
        //        ResizingThumb.Visibility = Visibility.Hidden;
        //    }
        //}
        private void BorderContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_IsResizing)
                return;

            if (_IsAutoResizing)
            {
                _IsAutoResizing = false;
                return;
            }

            var control = sender as TextBox;
            if (control == null)
                return;

            if (double.IsPositiveInfinity(control.DesiredSize.Width) ||
                double.IsPositiveInfinity(control.DesiredSize.Height) ||
                e.PreviousSize.Width == 0 || e.PreviousSize.Height == 0)
                return;

            var w = control.DesiredSize.Width / e.PreviousSize.Width;
            var h = control.DesiredSize.Height / e.PreviousSize.Height;
            BorderContent.Width *= w;
            BorderContent.Height *= h;

            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        private void BorderContent_GotFocus(object sender, RoutedEventArgs e)
        {
            //ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
        }
        private void BorderContent_LostFocus(object sender, RoutedEventArgs e)
        {
            //ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemDeselected(this);
        }
        #endregion

        #region update changes
        public override TemporalCurrentSnapCoordinates GetTemporalCurrentSnapCoordinates(Point p)
        {
            var coords = new TemporalCurrentSnapCoordinates();

            double w, h;
            double shapeW = 0;
            double shapeH = 0;
            if (double.IsNaN(BorderContent.Width))
            {
                w = BorderContent.RenderSize.Width + shapeW;
                h = BorderContent.RenderSize.Height + shapeH;
            }
            else
            {
                w = BorderContent.Width + shapeW;
                h = BorderContent.Height + shapeH;
            }
            w *= 0.5d;
            h *= 0.5d;
            coords.CenterX = p.X + w + RootGrid.ColumnDefinitions[0].ActualWidth;
            coords.CenterY = p.Y + h + RootGrid.RowDefinitions[0].ActualHeight;
            coords.Left = coords.CenterX - w;
            coords.Right = coords.CenterX + w;
            coords.Top = coords.CenterY - h;
            coords.Bottom = coords.CenterY + h;
            coords.ThLeft = coords.Left - (RootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
            coords.ThRight = coords.Right + (RootGrid.ColumnDefinitions[2].ActualWidth * 0.5d);
            coords.ThTop = coords.Top - (RootGrid.RowDefinitions[0].ActualHeight * 0.5d);
            coords.ThBottom = coords.Bottom + (RootGrid.RowDefinitions[2].ActualHeight * 0.5d);

            return coords;
        }
        internal override void CalculateSnapCoords(bool automatic = false)
        {
            if (!_IsMoving)
                ChartCustomControl.Instance.SnapToObjectsHandler.UpdateSnapRemoveBorder(this);

            CenterX = (AnchorPoint.X); //Canvas.GetLeft(this);
            CenterY = (AnchorPoint.Y); //Canvas.GetTop(this);

            double w, h;
            double shapeW = 0;
            double shapeH = 0;
            if (double.IsNaN(BorderContent.Width))
            {
                w = BorderContent.RenderSize.Width + shapeW;
                h = BorderContent.RenderSize.Height + shapeH;
            }
            else
            {
                w = BorderContent.Width + shapeW;
                h = BorderContent.Height + shapeH;
            }
            w *= 0.5d;
            h *= 0.5d;
            Left = new Point(CenterX - w, CenterY);
            Right = new Point(CenterX + w, CenterY);
            Top = new Point(CenterX, CenterY - h);
            Bottom = new Point(CenterX, CenterY + h);

            DoActionAllConnectingThumbs(x => x.MyBorderMovedTo(automatic));

            if (!_IsMoving)
                ChartCustomControl.Instance.SnapToObjectsHandler.UpdateSnapAddBorder(this);
        }
        internal override void UpdateAnchorPoint()
        {
            //Point ofs;
            //if(double.IsNaN(ContentView.Width))
            //{
            //    ofs = new Point(RenderSize.Width * 0.5d, RenderSize.Height * 0.5d);
            //}
            //else
            //    ofs = new Point(ContentView.Width * 0.5d, ContentView.Height * 0.5d);
            //AnchorPoint = ContentView.TranslatePoint(ofs, ChartCustomControl.Instance.ChartCanvas);
            Size size = RenderSize;
            Point ofs = new Point(size.Width / 2, size.Height / 2);
            AnchorPoint = this.TranslatePoint(ofs, ChartCustomControl.Instance.ChartCanvas);
            //AnchorPoint = new Point(Canvas.GetLeft(this) - ActualWidth * 0.5d, Canvas.GetTop(this) - ActualHeight * 0.5d);
        }
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            UpdateAnchorPoint();
            CalculateSnapCoords();
        }
        #endregion

        #region select by root grid
        private void RootGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }
        private void RootGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            e.Handled = true;
        }
        #endregion

        #region have thumbs
        private void ToggleVisibleThumbs()
        {
            if (_MovingThumbVisible)
            {
                DoActionAllConnectingThumbs(x => x.Visibility = Visibility.Visible);
                MovingThumb.Visibility = Visibility.Visible;

                ResizingThumb.Visibility = Visibility.Hidden;
                RotatingThumb.Visibility = Visibility.Hidden;
            }
            else
            {
                DoActionAllConnectingThumbs(x =>
                {
                    if (!x.IsChecked.Value)
                        x.Visibility = Visibility.Hidden;
                });
                MovingThumb.Visibility = Visibility.Hidden;

                ResizingThumb.Visibility = Visibility.Visible;
                RotatingThumb.Visibility = Visibility.Visible;
            }
        }

        public void HideAllThumbs()
        {
            if (ChartCustomControl.Instance._InternalShowAllHiddableThumbs)
                return;
            if (_MovingThumbVisible)
            {
                DoActionAllConnectingThumbs(x =>
                {
                    if (!x.IsChecked.Value)
                        x.Visibility = Visibility.Hidden;
                });
                if (!_IsMoving)
                    MovingThumb.Visibility = Visibility.Hidden;
            }
            else
            {
                if (!_IsResizing)
                    ResizingThumb.Visibility = Visibility.Hidden;
                if (!_IsRotating)
                    RotatingThumb.Visibility = Visibility.Hidden;
            }
        }
        public void ShowAllThumbs()
        {
            if (ChartCustomControl.Instance._InternalShowAllHiddableThumbs)
                return;
            //DoActionAllThumbs(x => x.Visibility = Visibility.Visible);
            if (_MovingThumbVisible)
            {
                DoActionAllConnectingThumbs(x => x.Visibility = Visibility.Visible);
                MovingThumb.Visibility = Visibility.Visible;
            }
            else
            {
                ResizingThumb.Visibility = Visibility.Visible;
                RotatingThumb.Visibility = Visibility.Visible;
            }
        }

        public IChartThumb GetThumbByType(EntityConnectingThumbTypesEnum type)
        {
            switch (type)
            {
                case EntityConnectingThumbTypesEnum.Left:
                    return ThLeft;
                case EntityConnectingThumbTypesEnum.Right:
                    return ThRight;
                case EntityConnectingThumbTypesEnum.Top:
                    return ThTop;
                case EntityConnectingThumbTypesEnum.Bottom:
                    return ThBottom;
            }
            return null;
        }
        public IEnumerable<IChartThumb> GetAllThumbs()
        {
            yield return ThLeft;
            yield return ThRight;
            yield return ThTop;
            yield return ThBottom;
            yield return MovingThumb;
            yield return ResizingThumb;
            yield return RotatingThumb;
            yield break;
        }
        public void DoActionAllThumbs(Action<IChartThumb> action)
        {
            foreach (var item in GetAllThumbs())
            {
                action(item);
            }
        }
        public IEnumerable<EntityConnectingThumb> GetAllConnectingThumbs()
        {
            yield return ThLeft;
            yield return ThRight;
            yield return ThTop;
            yield return ThBottom;
            yield break;
        }
        public void DoActionAllConnectingThumbs(Action<EntityConnectingThumb> action)
        {
            foreach (var item in GetAllConnectingThumbs())
            {
                action(item);
            }
        }
        #endregion

        #region remove this
        public void RemoveThis()
        {
            var parameters = new object[1] { this };
            if (!UndoRedoCommandManager.Instance.LastCommandIsNull)
                UndoRedoCommandManager.Instance.AddToLastCommand(null, null, x => RedoRemoveAction(x), parameters);
            if (IsSelected)
                ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemDeselected(this);
            DoActionAllConnectingThumbs(x =>
            {
                x.AddToLastCommandMyUndoRedoCommands();
                x.RemovedByParent();
            });
        }
        private void RedoRemoveAction(object[] parameters)
        {
            var border = parameters[0] as TextUserControl;
            border.DoActionAllConnectingThumbs(x => x.RemovedByParent());
        }
        #endregion

        private static SolidColorBrush _SelectedBrush;
        private static SolidColorBrush _UnselectedBrush;
        protected override void UpdateSelectedVisualEffect()
        {
            if (IsSelected)
            {
                SelectionBorderThickness = 2d;
                SelectionBorderBrush = _SelectedBrush;
                Canvas.SetLeft(this, Canvas.GetLeft(this) - 1);
                Canvas.SetTop(this, Canvas.GetTop(this) - 1);
            }
            else
            {
                SelectionBorderThickness = 1d;
                if (IsMouseOver)
                    SelectionBorderBrush = _UnselectedBrush;
                else
                    SelectionBorderBrush = Brushes.Transparent;
                Canvas.SetLeft(this, Canvas.GetLeft(this) + 1);
                Canvas.SetTop(this, Canvas.GetTop(this) + 1);
            }
        }

        #region equatable
        public override bool Equals(IVisualMoveRotate other)
        {
            return ((UserControl)this).Equals(other);
        }
        public override bool Equals(IVisualWithSnappingCoordinates other)
        {
            return ((UserControl)this).Equals(other);
        }
        public override int GetVisualHashCode()
        {
            return ((UserControl)this).GetHashCode();
        }
        #endregion
    }
}
