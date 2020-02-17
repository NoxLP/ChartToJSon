using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Entities.EntitiesShapesUserControls;
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
    public partial class TextWithShapeUserControl : 
        ChartEntityResizeMoveRotate, IChartObjectCanBeRemoved, IVisualWithConnectingThumbs, IVisualText
    {
        public TextWithShapeUserControl()
        {
            InitializeComponent();
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_EntityBorder);

            if (_ContentToShapeSizeConverter == null)
            {
                lock (_LockObject)
                {
                    if (_ContentToShapeSizeConverter == null)
                    {
                        //_SizeToContentMarginConverter = new Converters.EntityBorderSizeToContentMarginConverter();
                        _ShapeToBorderSizeConverter = new Converters.EntityBorderShapeToBorderSizeConverter();
                        _ContentToShapeSizeConverter = new Converters.EntityBorderContentToShapeSizeConverter();
                    }
                }
            }

            _RotateTransform = new RotateTransform();
            RenderTransform = _RotateTransform;
            RenderTransformOrigin = new Point(0.5, 0.5);
        }

        #region fields
        private static object _LockObject = new object();
        //private static Converters.EntityBorderSizeToContentMarginConverter _SizeToContentMarginConverter;
        private static Converters.EntityBorderShapeToBorderSizeConverter _ShapeToBorderSizeConverter;
        private static Converters.EntityBorderContentToShapeSizeConverter _ContentToShapeSizeConverter;
        private bool _MovingThumbVisible = true;
        #endregion

        #region properties
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
        public Shape MyShape
        {
            get { return (Shape)GetValue(MyShapeProperty); }
            set { SetValue(MyShapeProperty, value); }
        }
        public static readonly DependencyProperty MyShapeProperty =
            DependencyProperty.Register("MyShape", typeof(Shape), typeof(TextWithShapeUserControl), new PropertyMetadata(null, ShapeChanged));
        private static void ShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextWithShapeUserControl)d).MyShapeChanged((Shape)e.NewValue);
        }
        private void MyShapeChanged(Shape shape)
        {
            var tag = shape.Tag;
            if (tag is Thickness)
                ContentMargin = (Thickness)tag;
            else
                ContentMargin = new Thickness(0);

            var b = new Binding() { Source = this, Path = new PropertyPath(SelectionBorderBrushProperty) };
            BindingOperations.SetBinding(MyShape, Shape.StrokeProperty, b);
            b = new Binding() { Source = this, Path = new PropertyPath(SelectionBorderThicknessProperty) };
            BindingOperations.SetBinding(MyShape, Shape.StrokeThicknessProperty, b);
            b = new Binding() { Source = this, Path = new PropertyPath(BackgroundProperty) };
            BindingOperations.SetBinding(MyShape, Shape.FillProperty, b);
            b = new Binding("BackgroundBrush") { Source = DataContext };
            BindingOperations.SetBinding(MyShape, Shape.FillProperty, b);

            BindShapeSize();
        }

        public Brush MyShapeStrokeBrush
        {
            get { return (Brush)GetValue(MyShapeStrokeBrushProperty); }
            set { SetValue(MyShapeStrokeBrushProperty, value); }
        }
        public static readonly DependencyProperty MyShapeStrokeBrushProperty =
            DependencyProperty.Register("MyShapeStrokeBrush", typeof(Brush), typeof(TextWithShapeUserControl), new PropertyMetadata(Brushes.Black, ShapeStrokePropChanged));
        private static void ShapeStrokePropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextWithShapeUserControl)d).ShapeStrokeChanged();
        }
        private void ShapeStrokeChanged()
        {
            SelectionBorderBrush = (SolidColorBrush)MyShapeStrokeBrush;
        }

        public SolidColorBrush SelectionBorderBrush
        {
            get { return (SolidColorBrush)GetValue(SelectionBorderBrushProperty); }
            set { SetValue(SelectionBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty SelectionBorderBrushProperty =
            DependencyProperty.Register("SelectionBorderBrush", typeof(SolidColorBrush), typeof(TextWithShapeUserControl), new PropertyMetadata(Brushes.Black));

        private void BindThisSize()
        {
            var b = new Binding()
            {
                Source = MyShape,
                Path = new PropertyPath(ActualWidthProperty),
                Converter = _ShapeToBorderSizeConverter,
                ConverterParameter = new Tuple<bool, ChartEntityMoveRotate>(true, this),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(this, WidthProperty, b);
            b = new Binding()
            {
                Source = MyShape,
                Path = new PropertyPath(ActualHeightProperty),
                Converter = _ShapeToBorderSizeConverter,
                ConverterParameter = new Tuple<bool, ChartEntityMoveRotate>(false, this),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(this, HeightProperty, b);
        }
        public override void BindShapeSize()
        {
            var b = new Binding()
            {
                Source = BorderContent,
                Path = new PropertyPath(WidthProperty),
                Converter = _ContentToShapeSizeConverter,
                ConverterParameter = new Tuple<bool, ChartEntityMoveRotate>(true, this),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(MyShape, WidthProperty, b);
            b = new Binding()
            {
                Source = BorderContent,
                Path = new PropertyPath(HeightProperty),
                Converter = _ContentToShapeSizeConverter,
                ConverterParameter = new Tuple<bool, ChartEntityMoveRotate>(false, this),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(MyShape, HeightProperty, b);
        }
        #endregion

        #region other events
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (IVisualTextWithShapeViewModel)DataContext;
            vm.TextLoaded(this);
            Canvas.SetLeft(this, vm.CanvasX);// - AnchorPoint.X);
            Canvas.SetTop(this, vm.CanvasY);// - AnchorPoint.Y);
            var b = new Binding("Width") { Source = vm, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(BorderContent, Viewbox.WidthProperty, b);
            b = new Binding("Height") { Source = vm, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(BorderContent, Viewbox.HeightProperty, b);
            b = new Binding("Angle") { Source = vm, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(_RotateTransform, RotateTransform.AngleProperty, b);
            b = new Binding("ShapeStrokeBrush") { Source = vm, Mode = BindingMode.TwoWay };
            SetBinding(TextWithShapeUserControl.MyShapeStrokeBrushProperty, b);
            if (vm.ShapeStrokeBrush == null)
                vm.ShapeStrokeBrush = Brushes.Black;

            //AnchorPoint = new Point(vm.CanvasX - ActualWidth * 0.5d, vm.CanvasY - ActualHeight * 0.5d);
            var uc = BorderContent;
            if (vm.Width == 0)
            {
                BorderContent.Width = uc.ActualWidth;
                BorderContent.Height = uc.ActualHeight;
            }
            UpdateAnchorPoint();
            DoActionAllConnectingThumbs(x => x.InitAnchorPoint());

            //InitCalculateSnapCoords();
            CalculateSnapCoords();

            b = new Binding(nameof(IChartEntityViewModel.ShapeTemplateKey)) { Source = vm, Converter = ChartCustomControl.Instance.EntityShapeSelector };
            SetBinding(MyShapeProperty, b);

            ((IChartMainVM)ChartCustomControl.Instance.DataContext).CheckForPastedConnections(vm);
        }
        private void RootGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            ChartCustomControl.Instance._MouseEnterOnVisual++;
            ShowAllThumbs();
            Application.Current.MainWindow.KeyDown += KeyDown;
            Application.Current.MainWindow.KeyUp += KeyUp;
        }
        private void RootGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            ChartCustomControl.Instance._MouseEnterOnVisual--;
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
            ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
        }
        private void BorderContent_LostFocus(object sender, RoutedEventArgs e)
        {
            ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemDeselected(this);
        }
        #endregion

        #region update changes
        public override TemporalCurrentSnapCoordinates GetTemporalCurrentSnapCoordinates(Point p)
        {
            var coords = new TemporalCurrentSnapCoordinates();

            //Size size = RenderSize;
            //Point ofs = new Point(size.Width / 2, size.Height / 2); //isInput ? 0 : size.Height);
            //var p = this.TranslatePoint(ofs, ChartCustomControl.Instance.ChartCanvas);

            //var bcSize = new Point(BorderContent.RenderSize.Width, BorderContent.RenderSize.Height);
            //var bcPoint = this.TranslatePoint(bcSize, ChartCustomControl.Instance.ChartCanvas);

            coords.CenterX = (p.X); //Canvas.GetLeft(this);
            coords.CenterY = (p.Y); //Canvas.GetTop(this);
            coords.Left = (p.X - (BorderContent.RenderSize.Width * 0.5d));
            coords.Right = (p.X + (BorderContent.RenderSize.Width * 0.5d));
            coords.Top = (p.Y - (BorderContent.RenderSize.Height * 0.5d));
            coords.Bottom = (p.Y + (BorderContent.RenderSize.Height * 0.5d));
            coords.ThLeft = p.X - (BorderContent.ActualWidth * 0.5d) - (RootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
            coords.ThRight = p.X + (BorderContent.ActualWidth * 0.5d) + (RootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
            coords.ThTop = p.Y - (BorderContent.ActualHeight * 0.5d) - (RootGrid.RowDefinitions[0].ActualHeight * 0.5d);
            coords.ThBottom = p.Y + (BorderContent.ActualHeight * 0.5d) + (RootGrid.RowDefinitions[0].ActualHeight * 0.5d);

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
            if (MyShape != null)
            {
                shapeW = ContentMargin.Right + ContentMargin.Left;
                shapeH = ContentMargin.Top + ContentMargin.Bottom;
            }
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
            var border = parameters[0] as EntityBorderUserControl;
            border.DoActionAllConnectingThumbs(x => x.RemovedByParent());
        }
        #endregion

        protected override void UpdateSelectedVisualEffect()
        {
            if (IsSelected)
            {
                SelectionBorderThickness = 2d;
                SelectionBorderBrush = (SolidColorBrush)FindResource("SelectionBorderSelectedBrush");
                Canvas.SetLeft(this, Canvas.GetLeft(this) - 1);
                Canvas.SetTop(this, Canvas.GetTop(this) - 1);
            }
            else
            {
                SelectionBorderThickness = 1d;
                if (MyShapeStrokeBrush == null)
                    SelectionBorderBrush = (SolidColorBrush)FindResource("SelectionBorderUnSelectedBrush");
                else
                    SelectionBorderBrush = (SolidColorBrush)MyShapeStrokeBrush;
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
