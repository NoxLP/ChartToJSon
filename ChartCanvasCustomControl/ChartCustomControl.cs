using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Lines;
using ChartCanvasNamespace.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChartCanvasNamespace.ZoomAndPanModel;
using WPFHelpers.Helpers;
using ChartCanvasNamespace.Thumbs;
using WPFHelpers.CancelActions;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.ComponentModel;
using ChartCanvasNamespace.OtherVisuals;
using ChartCanvasNamespace.VisualsBase;
using System.Windows.Data;
using System.Collections;

namespace ChartCanvasNamespace
{
    /// <summary>
    /// Realice los pasos 1a o 1b y luego 2 para usar este control personalizado en un archivo XAML.
    ///
    /// Paso 1a) Usar este control personalizado en un archivo XAML existente en el proyecto actual.
    /// Agregue este atributo XmlNamespace al elemento raíz del archivo de marcado en el que 
    /// se va a utilizar:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChartCanvasCustomControl"
    ///
    ///
    /// Paso 1b) Usar este control personalizado en un archivo XAML existente en otro proyecto.
    /// Agregue este atributo XmlNamespace al elemento raíz del archivo de marcado en el que 
    /// se va a utilizar:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChartCanvasCustomControl;assembly=ChartCanvasCustomControl"
    ///
    /// Tendrá también que agregar una referencia de proyecto desde el proyecto en el que reside el archivo XAML
    /// hasta este proyecto y recompilar para evitar errores de compilación:
    ///
    ///     Haga clic con el botón secundario del mouse en el proyecto de destino en el Explorador de soluciones y seleccione
    ///     "Agregar referencia"->"Proyectos"->[seleccione este proyecto]
    ///
    ///
    /// Paso 2)
    /// Prosiga y utilice el control en el archivo XAML.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class ChartCustomControl : Control, INotifyPropertyChanged
    {
        internal class DragBoxesData
        {
            public DragBoxesData(Point pt1, Point pt2)
            {
                //
                // Deterine x,y,width and height of the rect inverting the points if necessary.
                // 

                if (pt2.X < pt1.X)
                {
                    x = pt2.X;
                    width = pt1.X - pt2.X;
                }
                else
                {
                    x = pt1.X;
                    width = pt2.X - pt1.X;
                }

                if (pt2.Y < pt1.Y)
                {
                    y = pt2.Y;
                    height = pt1.Y - pt2.Y;
                }
                else
                {
                    y = pt1.Y;
                    height = pt2.Y - pt1.Y;
                }
            }

            internal double x, y, width, height;
        }

        #region constructors/init
        static ChartCustomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartCustomControl), new FrameworkPropertyMetadata(typeof(ChartCustomControl)));
        }
        public ChartCustomControl()
        {
            _CurrentEntityWithVisibleThumbs = new List<IChartHaveHiddableThumbs>();
            SnapToObjectsHandler = new SnapToObjectsHandlerClass();
            ChartItemsSelectionHandler = new ChartItemsSelectionHandlerClass(this);
            EntityContentTemplateSelector = new Converters.EntityContentDataTemplateSelector();
            //EntityShapeTemplateSelector = new Converters.EntityShapeDataTemplateSelector();
            EntityShapeSelector = new Converters.EntityShapeSelectorConverter();
            var resourceDict = new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/ChartCanvas;component/Resources/ResDict0.xaml") };
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            EntityContentTemplateSelector.DefaultTemplate = (DataTemplate)Application.Current.FindResource("DefaultEntityShapeDataTemplate");
            //EntityShapeTemplateSelector.DefaultTemplate = (DataTemplate)resourceDict["DefaultEntityShapeDataTemplate"];
            EntityShapeSelector.DefaultTemplate = (DataTemplate)Application.Current.FindResource("DefaultEntityShapeDataTemplate");
            VisualItemsSelected = new ObservableCollection<IVisualCanBeSelected>();
            VisualsWithShapeSelected = new ObservableCollection<IVisualWithShapeViewModel>();
            VisualsWithBackgroundSelected = new ObservableCollection<IVisualWithBackground>();
            ChartEntitiesSelected = new ObservableCollection<IChartEntityViewModel>();
            VisualTextsSelected = new ObservableCollection<IVisualTextViewModel>();
            //ZoomAndPan = new ZoomAndPanControl();
            Application.Current.MainWindow.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }
        public override void OnApplyTemplate()
        {
            var dc = DataContext as IChartMainVM;
            if (dc == null)
                throw new ArgumentException("Data context of ChartCustomControl must implement IChartMainVM");

            base.OnApplyTemplate();

            ChartCanvas = GetTemplateChild("PART_CCanvas") as Canvas;
            ChartCanvas.Loaded += ChartCanvas_Loaded;
            _Origin = new Point(0.5, 0.5);
            ChartCanvas.RenderTransformOrigin = _Origin;
            ChartCanvas.SizeChanged += ChartCanvas_SizeChanged;
            Panel.SetZIndex(ChartCanvas, 0);
            //ZoomAndPan = GetTemplateChild("PART_ZoomAndPanControl") as ZoomAndPanControl;
            _SelectionBoxCanvas = GetTemplateChild("PART_SelectionBoxCanvas") as Canvas;
            _SelectionBox = GetTemplateChild("PART_SelectionBox") as Border;
            //_ZoomBoxCanvas = GetTemplateChild("PART_ZoomBoxCanvas") as Canvas;
            //_ZoomBox = GetTemplateChild("PART_ZoomBox") as Border;
            _CanvasResizeThumb = GetTemplateChild("PART_ResizeThumb") as Thumb;
            _CanvasResizeThumb.DragDelta += _CanvasResizeThumb_DragDelta;
            Panel.SetZIndex(_SelectionBox, Properties.Settings.Default.ZIndex_Boxes);
            //Panel.SetZIndex(_ZoomBox, Properties.Settings.Default.ZIndex_Boxes);

            HorizontalSnapLine = GetTemplateChild("PART_SnapHorizontalLine") as Rectangle;
            VerticalSnapLine = GetTemplateChild("PART_SnapVerticalLine") as Rectangle;
            Panel.SetZIndex(HorizontalSnapLine, Properties.Settings.Default.ZIndex_SnapLine);
            Panel.SetZIndex(VerticalSnapLine, Properties.Settings.Default.ZIndex_SnapLine);

            //MouseDown += zoomAndPanControl_MouseDown;
            //MouseUp += zoomAndPanControl_MouseUp;
            //MouseMove += zoomAndPanControl_MouseMove;
            //MouseWheel += zoomAndPanControl_MouseWheel;
            //MouseDoubleClick += zoomAndPanControl_MouseDoubleClick;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnNormalPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        #region control stuff
        #region fields
        private readonly string _CancellableActionsToken = "ChCuCo";
        private object _LockObject = new object();
        internal Canvas _SelectionBoxCanvas;
        internal Border _SelectionBox;
        //private Canvas _ZoomBoxCanvas;
        //private Border _ZoomBox;
        private Thumb _CanvasResizeThumb;
        private bool _SomeSelectedVisualTextHasShape;
        private bool _SomeItemIsSelected;
        private bool _SomeChartItemIsSelected;
        private bool _SomeVisualTextIsSelected;
        private bool _SomeResizeRotateMoveItemIsSelected;
        private bool _SomeItemWithShapeIsSelected;
        #endregion

        #region properties
        public static ChartCustomControl Instance { get; private set; }

        public Converters.EntityContentDataTemplateSelector EntityContentTemplateSelector { get; private set; }
        //public Converters.EntityShapeDataTemplateSelector EntityShapeTemplateSelector { get; private set; }
        internal Converters.EntityShapeSelectorConverter EntityShapeSelector { get; private set; }
        public Canvas ChartCanvas { get; private set; }
        //public ZoomAndPanControl ZoomAndPan { get; private set; }
        public SnapToObjectsHandlerClass SnapToObjectsHandler { get; private set; }
        public ChartItemsSelectionHandlerClass ChartItemsSelectionHandler { get; private set; }

        public Rectangle HorizontalSnapLine { get; private set; }
        public Rectangle VerticalSnapLine { get; private set; }

        public bool SomeItemIsSelected
        {
            get { return _SomeItemIsSelected; }
            set
            {
                if (value != _SomeItemIsSelected)
                {
                    _SomeItemIsSelected = value;
                    OnNormalPropertyChanged(nameof(SomeItemIsSelected));
                }
            }
        }
        public bool SomeChartItemIsSelected
        {
            get { return _SomeChartItemIsSelected; }
            set
            {
                if (value != _SomeChartItemIsSelected)
                {
                    _SomeChartItemIsSelected = value;
                    OnNormalPropertyChanged(nameof(SomeChartItemIsSelected));
                }
            }
        }
        public bool SomeVisualTextIsSelected
        {
            get { return _SomeVisualTextIsSelected; }
            set
            {
                if (value != _SomeVisualTextIsSelected)
                {
                    _SomeVisualTextIsSelected = value;
                    OnNormalPropertyChanged(nameof(SomeVisualTextIsSelected));
                }
            }
        }
        public bool SomeResizeRotateMoveItemIsSelected
        {
            get { return _SomeResizeRotateMoveItemIsSelected; }
            set
            {
                if (value != _SomeResizeRotateMoveItemIsSelected)
                {
                    _SomeResizeRotateMoveItemIsSelected = value;
                    OnNormalPropertyChanged(nameof(SomeResizeRotateMoveItemIsSelected));
                }
            }
        }
        public bool SomeItemWithShapeIsSelected
        {
            get { return _SomeItemWithShapeIsSelected; }
            set
            {
                if (value != _SomeItemWithShapeIsSelected)
                {
                    _SomeItemWithShapeIsSelected = value;
                    OnNormalPropertyChanged(nameof(SomeItemWithShapeIsSelected));
                }
            }
        }
        #endregion

        #region dependency properties
        public ObservableCollection<IVisualCanBeSelected> VisualItemsSelected
        {
            get { return (ObservableCollection<IVisualCanBeSelected>)GetValue(VisualItemsSelectedProperty); }
            set { SetValue(VisualItemsSelectedProperty, value); }
        }
        public static readonly DependencyProperty VisualItemsSelectedProperty =
            DependencyProperty.Register("VisualItemsSelected", typeof(ObservableCollection<IVisualCanBeSelected>), typeof(ChartCustomControl), new PropertyMetadata(
                null, VisualCISChanged));
        private static void VisualCISChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChartCustomControl)d).VisualChartItemsSelectedCollsChanged(e);
        }
        private void VisualChartItemsSelectedCollsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += VisualItemsSelectedChanged;
            }
            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= VisualItemsSelectedChanged;
            }
        }
        private void VisualItemsSelectedChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (VisualItemsSelected.Count > 0)
            {
                if (!SomeItemIsSelected)
                    SomeItemIsSelected = true;
            }
            else
            {
                if (SomeItemIsSelected && !SomeVisualTextIsSelected && !SomeChartItemIsSelected)
                    SomeItemIsSelected = false;
            }
        }

        public ObservableCollection<IVisualWithBackground> VisualsWithBackgroundSelected
        {
            get { return (ObservableCollection<IVisualWithBackground>)GetValue(VisualsWithBackgroundSelectedProperty); }
            set { SetValue(VisualsWithBackgroundSelectedProperty, value); }
        }
        public static readonly DependencyProperty VisualsWithBackgroundSelectedProperty =
            DependencyProperty.Register("VisualsWithBackgroundSelected", typeof(ObservableCollection<IVisualWithBackground>), typeof(ChartCustomControl), new PropertyMetadata(null));

        public ObservableCollection<IVisualWithShapeViewModel> VisualsWithShapeSelected
        {
            get { return (ObservableCollection<IVisualWithShapeViewModel>)GetValue(VisualsWithShapeSelectedProperty); }
            set { SetValue(VisualsWithShapeSelectedProperty, value); }
        }
        public static readonly DependencyProperty VisualsWithShapeSelectedProperty =
            DependencyProperty.Register("VisualsWithShapeSelected", typeof(ObservableCollection<IVisualWithShapeViewModel>), typeof(ChartCustomControl), new PropertyMetadata(null));
        private void FilterItemsWithShapeAndBackgroundFromSelectedItems(IList newItems, IList oldItems)
        {
            if (newItems != null && newItems.Count > 0)
            {
                foreach (var item in newItems)
                {
                    var withShape = item as IVisualWithShapeViewModel;
                    if (withShape != null && !VisualsWithShapeSelected.Contains(withShape))
                        VisualsWithShapeSelected.Add(withShape);
                    var withBackground = item as IVisualWithBackground;
                    if (withBackground != null && !VisualsWithBackgroundSelected.Contains(withBackground))
                        VisualsWithBackgroundSelected.Add(withBackground);
                }
            }
            if (oldItems != null &&  oldItems.Count > 0)
            {
                foreach (var item in oldItems)
                {
                    var withShape = item as IVisualWithShapeViewModel;
                    if (withShape != null)
                        VisualsWithShapeSelected.Remove(withShape);
                    var withBackground = item as IVisualWithBackground;
                    if (withBackground != null)
                        VisualsWithBackgroundSelected.Remove(withBackground);
                }
            }
        }
        
        public ObservableCollection<IVisualTextViewModel> VisualTextsSelected
        {
            get { return (ObservableCollection<IVisualTextViewModel>)GetValue(VisualTextsSelectedProperty); }
            set { SetValue(VisualTextsSelectedProperty, value); }
        }
        public static readonly DependencyProperty VisualTextsSelectedProperty =
            DependencyProperty.Register("VisualTextsSelected", typeof(ObservableCollection<IVisualTextViewModel>), typeof(ChartCustomControl), new PropertyMetadata(
                null, TISChanged));
        private static void TISChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChartCustomControl)d).TextItemsSelectedCollsChanged(e);
        }
        private void TextItemsSelectedCollsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += TextItemsSelectedChanged;
            }
            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= TextItemsSelectedChanged;
            }
        }
        private void TextItemsSelectedChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(VisualTextsSelected.Count > 0)
            {
                _SomeSelectedVisualTextHasShape = VisualTextsSelected.Any(x => x is IVisualTextWithShapeViewModel);
                if (!SomeVisualTextIsSelected)
                    SomeVisualTextIsSelected = true;
                if (!SomeItemIsSelected)
                    SomeItemIsSelected = true;
                if (!SomeResizeRotateMoveItemIsSelected)
                    SomeResizeRotateMoveItemIsSelected = true;

                if (_SomeSelectedVisualTextHasShape)
                    SomeItemWithShapeIsSelected = true;
                else if (SomeItemWithShapeIsSelected && !SomeChartItemIsSelected)
                    SomeItemWithShapeIsSelected = false;
            }
            else
            {
                _SomeSelectedVisualTextHasShape = false;
                if (SomeVisualTextIsSelected)
                    SomeVisualTextIsSelected = false;
                if (!SomeChartItemIsSelected)
                {
                    if (SomeItemIsSelected && VisualItemsSelected.Count == 0)
                        SomeItemIsSelected = false;
                    if (SomeResizeRotateMoveItemIsSelected)
                        SomeResizeRotateMoveItemIsSelected = false;
                }

                if (SomeItemWithShapeIsSelected && !SomeChartItemIsSelected)
                    SomeItemWithShapeIsSelected = false;
            }
            FilterItemsWithShapeAndBackgroundFromSelectedItems(e.NewItems, e.OldItems);
        }

        public ObservableCollection<IChartEntityViewModel> ChartEntitiesSelected
        {
            get { return (ObservableCollection<IChartEntityViewModel>)GetValue(ChartEntitiesSelectedProperty); }
            set { SetValue(ChartEntitiesSelectedProperty, value); }
        }
        public static readonly DependencyProperty ChartEntitiesSelectedProperty =
            DependencyProperty.Register("ChartEntitiesSelected", typeof(ObservableCollection<IChartEntityViewModel>), typeof(ChartCustomControl), new PropertyMetadata(
                null, CISChanged));
        private static void CISChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChartCustomControl)d).ChartItemsSelectedCollsChanged(e);
        }
        private void ChartItemsSelectedCollsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += SelectedEntitiesChanged;
            }
            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= SelectedEntitiesChanged;
            }
        }
        private void SelectedEntitiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ChartEntitiesSelected.Count > 0)
            {
                if (!SomeChartItemIsSelected)
                    SomeChartItemIsSelected = true;
                if (!SomeItemIsSelected)
                    SomeItemIsSelected = true;
                if (!SomeResizeRotateMoveItemIsSelected)
                    SomeResizeRotateMoveItemIsSelected = true;
                if (!SomeItemWithShapeIsSelected)
                    SomeItemWithShapeIsSelected = true;
            }
            else
            {
                if (SomeChartItemIsSelected)
                    SomeChartItemIsSelected = false;
                if (!SomeVisualTextIsSelected)
                {
                    if (SomeItemIsSelected && VisualItemsSelected.Count == 0)
                        SomeItemIsSelected = false;
                    if (SomeResizeRotateMoveItemIsSelected)
                        SomeResizeRotateMoveItemIsSelected = false;
                }

                if (SomeItemWithShapeIsSelected && !_SomeSelectedVisualTextHasShape)
                    SomeItemWithShapeIsSelected = false;
            }
            FilterItemsWithShapeAndBackgroundFromSelectedItems(e.NewItems, e.OldItems);
        }

        public ObservableCollectionRange<IChartEntityViewModel> EntitiesViewModels
        {
            get { return (ObservableCollectionRange<IChartEntityViewModel>)GetValue(EntitiesViewModelsProperty); }
            set { SetValue(EntitiesViewModelsProperty, value); }
        }
        public static readonly DependencyProperty EntitiesViewModelsProperty =
            DependencyProperty.Register("EntitiesViewModels", typeof(ObservableCollectionRange<IChartEntityViewModel>), typeof(ChartCustomControl), new PropertyMetadata(
                null, EntitiesViewModelsChanged));
        private static void EntitiesViewModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChartCustomControl)d).ViewModelsCollChanged(e);
        }
        private void ViewModelsCollChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var coll = (INotifyRangeCollectionChanged)e.NewValue;
                //coll.CollectionChanged += ItemsCollectionChanged;
                coll.CollectionChangedRange += ItemsCollectionChanged;
            }
            if (e.OldValue != null)
            {
                var coll = (INotifyRangeCollectionChanged)e.OldValue;
                //coll.CollectionChanged -= ItemsCollectionChanged;
                coll.CollectionChangedRange -= ItemsCollectionChanged;
            }
        }

        public ObservableCollectionRange<IVisualTextViewModel> VisualsTextsViewModels
        {
            get { return (ObservableCollectionRange<IVisualTextViewModel>)GetValue(VisualsTextsViewModelsProperty); }
            set { SetValue(VisualsTextsViewModelsProperty, value); }
        }
        public static readonly DependencyProperty VisualsTextsViewModelsProperty =
            DependencyProperty.Register("VisualsTextsViewModels", typeof(ObservableCollectionRange<IVisualTextViewModel>), typeof(ChartCustomControl), new PropertyMetadata(
                null, VisualsTextsViewModelsChanged));
        private static void VisualsTextsViewModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChartCustomControl)d).VisualsTextsViewModelsCollChanged(e);
        }
        private void VisualsTextsViewModelsCollChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var coll = (INotifyRangeCollectionChanged)e.NewValue;
                //coll.CollectionChanged += ItemsCollectionChanged;
                coll.CollectionChangedRange += VisualTextsCollectionChanged;
            }
            if (e.OldValue != null)
            {
                var coll = (INotifyRangeCollectionChanged)e.OldValue;
                //coll.CollectionChanged -= ItemsCollectionChanged;
                coll.CollectionChangedRange -= VisualTextsCollectionChanged;
            }
        }
        #endregion

        private Point GetCanvasCenter()
        {
            return new Point(
                ChartCanvas.ActualWidth * 0.5d,
                ChartCanvas.ActualHeight * 0.5d);
        }
        private void HandleKeyDown(Key key)
        {
            switch (key)
            {
                case Key.OemMinus:
                case Key.Subtract:
                    Zoom(GetCanvasCenter(), -0.1);
                    break;
                case Key.OemPlus:
                case Key.Add:
                    Zoom(GetCanvasCenter(), 0.1);
                    break;
                //case Key.Back:
                //if (JumpBackToPrevZoom_CanExecuted())
                //    JumpBackToPrevZoom();
                //break;
                case Key.Delete:
                    var vm = ((IChartMainVM)DataContext);
                    vm.RemoveSelectedEntities();
                    break;
                case Key.Escape:
                    vm = ((IChartMainVM)DataContext);
                    if (!string.IsNullOrEmpty(vm.VMCancellableActionsToken))
                    {
                        CancellableActionsHandlerClass.Instance.CancelAllOnGoingActions(x =>
                            x.CancellableId.Contains(_CancellableActionsToken) || x.CancellableId.Contains(vm.VMCancellableActionsToken));
                    }
                    else
                        CancellableActionsHandlerClass.Instance.CancelAllOnGoingActions(x => x.CancellableId.Contains(_CancellableActionsToken));
                    Keyboard.ClearFocus();
                    break;
            }
        }
        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!IsKeyboardFocused || !FocusManager.GetFocusedElement(Application.Current.MainWindow).Equals(this))
            {
                return;
            }
            HandleKeyDown(e.Key);
            e.Handled = true;
        }
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            HandleKeyDown(e.Key);
            base.OnPreviewKeyDown(e);
        }

        #region add/remove items
        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var newItems = e.NewItems.Cast<IChartEntityViewModel>();
                HandleNewItems(newItems);
            }
            if (e.OldItems != null)
            {
                var oldItems = e.OldItems.Cast<IChartEntityViewModel>();
                HandleOldItems(oldItems);
            }
        }
        private void VisualTextsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var newItems = e.NewItems.Cast<IVisualTextViewModel>();
                HandleNewItems(newItems);
            }
            if (e.OldItems != null)
            {
                var oldItems = e.OldItems.Cast<IVisualTextViewModel>();
                HandleOldItems(oldItems);
            }
        }
        internal void HandleNewItems(IEnumerable<IChartEntityViewModel> items)
        {
            EntityBorderUserControl border;
            foreach (var vm in items)
            {
                if (vm.UserControl == null)
                    border = new EntityBorderUserControl() { DataContext = vm };
                else
                {
                    border = (EntityBorderUserControl)vm.UserControl;
                    border.DataContext = vm;
                }
                //border = vm.UserControl == null ? new EntityBorderUserControl() { DataContext = vm } : (EntityBorderUserControl)vm.UserControl; //vm.UserControl;
                //border.DataContext = vm;
                ChartCanvas.Children.Add(border);
                NewVisualWithVisibleThumbs(border);
            }
        }
        internal void HandleNewItems(IEnumerable<IVisualTextViewModel> items)
        {
            foreach (IVisualTextViewModel vm in items)
            {
                if (vm.Type == ChartEntityTypeEnum.TextEntity)
                {
                    TextWithShapeUserControl uc;
                    if (vm.UserControl == null)
                    {
                        uc = new TextWithShapeUserControl() { DataContext = vm };
                    }
                    else
                    {
                        uc = (TextWithShapeUserControl)vm.UserControl;
                        uc.DataContext = vm;
                    }
                    ChartCanvas.Children.Add(uc);
                    NewVisualWithVisibleThumbs(uc);
                }
                else
                {
                    TextUserControl uc;
                    if (vm.UserControl == null)
                    {
                        uc = new TextUserControl() { DataContext = vm };
                    }
                    else
                    {
                        uc = (TextUserControl)vm.UserControl;
                        uc.DataContext = vm;
                    }
                    ChartCanvas.Children.Add(uc);
                    NewVisualWithVisibleThumbs(uc);
                }
            }
        }
        private void HandleOldItems(IEnumerable<IChartEntityViewModel> items)
        {
            foreach (var vm in items)
            {
                SnapToObjectsHandler.UpdateSnapRemoveBorder(vm);
                //Remove lines, dividers, draggers, etc
                var border = (EntityBorderUserControl)vm.UserControl;
                border.RemoveThis();
                ChartCanvas.Children.Remove(border);
                NewVisualWithVisibleThumbs(border);
            }
        }
        private void HandleOldItems(IEnumerable<IVisualTextViewModel> items)
        {
            foreach (var vm in items)
            {
                SnapToObjectsHandler.UpdateSnapRemoveBorder(vm);
                //Remove lines, dividers, draggers, etc
                if (vm.Type == ChartEntityTypeEnum.TextEntity)
                {
                    var border = (TextWithShapeUserControl)vm.UserControl;
                    border.RemoveThis();
                    ChartCanvas.Children.Remove(border);
                }
                else
                {
                    var border = (TextUserControl)vm.UserControl;
                    border.RemoveThis();
                    ChartCanvas.Children.Remove(border);
                }
            }
        }
        public void ResizeSelectedEntities(double width, double height)
        {
            if (ChartEntitiesSelected.Count != 0)
                ((EntityBorderUserControl)ChartEntitiesSelected[0].UserControl).ResizeTo(width, height);
            else if (VisualTextsSelected.Count != 0)
                ((ChartEntityResizeMoveRotate)VisualTextsSelected[0].UserControl).ResizeTo(width, height);
        }
        #endregion

        #region helpers
        public static void SetMainInstance(ChartCustomControl chartCustomControl)
        {
            Instance = chartCustomControl;
            //BindingOperations.GetBindingExpression(Instance, SnapToOtherEntitiesProperty).UpdateSource();
            //BindingOperations.GetBindingExpression(Instance, SnapToConnectionAnchorPointsProperty).UpdateSource();
        }
        public void SetEntitiesContentTemplates(Dictionary<string, DataTemplate> templatesDictionary)
        {
            Converters.EntityContentDataTemplateSelector.AddTemplates(templatesDictionary);
        }
        public void SetEntitiesShapes(Dictionary<string, Shape> shapesDictionary)
        {
            Converters.EntityShapeSelectorConverter.AddTemplates(shapesDictionary);
        }
        #endregion
        #endregion

        #region canvas stuff
        #region fields
        private List<IChartHaveHiddableThumbs> _CurrentEntityWithVisibleThumbs;
        #endregion

        #region dependency properties
        public bool ShowGrid
        {
            get { return (bool)GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }
        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register("ShowGrid", typeof(bool), typeof(ChartCustomControl), new PropertyMetadata(false));

        public double GridLength
        {
            get { return (double)GetValue(GridLengthProperty); }
            set { SetValue(GridLengthProperty, value); }
        }
        public static readonly DependencyProperty GridLengthProperty =
            DependencyProperty.Register("GridLength", typeof(double), typeof(ChartCustomControl), new PropertyMetadata(50d));

        public Brush GridBrush
        {
            get { return (Brush)GetValue(GridBrushProperty); }
            set { SetValue(GridBrushProperty, value); }
        }
        public static readonly DependencyProperty GridBrushProperty =
            DependencyProperty.Register("GridBrush", typeof(Brush), typeof(ChartCustomControl), new PropertyMetadata(SystemColors.ActiveBorderBrush));

        public bool SnapToGrid
        {
            get { return (bool)GetValue(SnapToGridProperty); }
            set { SetValue(SnapToGridProperty, value); }
        }
        public static readonly DependencyProperty SnapToGridProperty =
            DependencyProperty.Register("SnapToGrid", typeof(bool), typeof(ChartCustomControl), new PropertyMetadata(false));

        public bool SnapToOtherEntities
        {
            get { return (bool)GetValue(SnapToOtherEntitiesProperty); }
            set { SetValue(SnapToOtherEntitiesProperty, value); }
        }
        public static readonly DependencyProperty SnapToOtherEntitiesProperty =
            DependencyProperty.Register("SnapToOtherEntities", typeof(bool), typeof(ChartCustomControl), new PropertyMetadata(true, SnapToEntitiesPropertyChanged));
        private static void SnapToEntitiesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                ((ChartCustomControl)d).SetValue(SnapToConnectionAnchorPointsProperty, false);
        }

        public bool SnapToConnectionAnchorPoints
        {
            get { return (bool)GetValue(SnapToConnectionAnchorPointsProperty); }
            set { SetValue(SnapToConnectionAnchorPointsProperty, value); }
        }
        public static readonly DependencyProperty SnapToConnectionAnchorPointsProperty =
            DependencyProperty.Register("SnapToConnectionAnchorPoints", typeof(bool), typeof(ChartCustomControl), new PropertyMetadata(true, SnapToAnchorPointsPropertyChanged));
        private static void SnapToAnchorPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                ((ChartCustomControl)d).SetValue(SnapToOtherEntitiesProperty, true);
        }

        internal bool _InternalShowAllHiddableThumbs;
        public bool ShowAllHiddableThumbs
        {
            get { return (bool)GetValue(ShowAllHiddableThumbsProperty); }
            set { SetValue(ShowAllHiddableThumbsProperty, value); }
        }
        public static readonly DependencyProperty ShowAllHiddableThumbsProperty =
            DependencyProperty.Register("ShowAllHiddableThumbs", typeof(bool), typeof(ChartCustomControl), new PropertyMetadata(false, ShowAllHiddableThumbsChanged));
        private static void ShowAllHiddableThumbsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = ((ChartCustomControl)d);
            if ((bool)e.NewValue)
            {
                chart.ShowAllThumbsIfAny();
                chart._InternalShowAllHiddableThumbs = true;
            }
            else
            {
                chart._InternalShowAllHiddableThumbs = false;
                chart.HideAllThumbsIfAny();
            }
        }

        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }
        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(ChartCustomControl), new PropertyMetadata(1200d));

        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }
        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(ChartCustomControl), new PropertyMetadata(600d));
        #endregion

        private void _CanvasResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CanvasWidth += e.HorizontalChange;
            CanvasHeight += e.VerticalChange;
        }
        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var borrame = ChartCanvas.ActualWidth;
            Canvas.SetLeft(_CanvasResizeThumb, ChartCanvas.ActualWidth - _CanvasResizeThumb.ActualWidth - 5);
            Canvas.SetTop(_CanvasResizeThumb, ChartCanvas.ActualHeight - _CanvasResizeThumb.ActualHeight - 5);
        }
        private void ChartCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            _TranslateTransform = new TranslateTransform();
            _ScaleTransform = new ScaleTransform();
            _TransformGroup = new TransformGroup();
            _TransformGroup.Children.Add(_ScaleTransform);
            _TransformGroup.Children.Add(_TranslateTransform);

            ChartCanvas.RenderTransform = _TransformGroup;
            Scale = _ScaleTransform.ScaleX;
            ZoomToFillScreen();
        }

        #region thumbs
        public void NewVisualWithVisibleThumbs(IChartHaveHiddableThumbs visual)
        {
            _CurrentEntityWithVisibleThumbs.Add(visual);
            visual.HideAllThumbs();
        }
        public void HideAllThumbsIfAny()
        {
            foreach (var item in _CurrentEntityWithVisibleThumbs)
            {
                item.HideAllThumbs();
            }
        }
        public void ShowAllThumbsIfAny()
        {
            foreach (var item in _CurrentEntityWithVisibleThumbs)
            {
                item.ShowAllThumbs();
            }
        }

        public IChartConnecterThumb LineStartThumb { get; private set; }
        internal void ConnecterChecked(IChartConnecterThumb connecter)
        {
            if(LineStartThumb == null)
            {
                LineStartThumb = connecter;
                connecter.SetNewLineCancellableAction();
            }
            else
            {
                if (LineStartThumb.Equals(connecter) || //same thumb => impossible?
                    (connecter.ConnecterType == ConnecterThumbTypesEnum.Line && LineStartThumb.ConnecterType == ConnecterThumbTypesEnum.Line))//two line connecters => nothing happens
                {
                    CancellableActionsHandlerClass.Instance.CancelObjectOnGoingAction(LineStartThumb);
                    connecter.ScriptUnchecked = true;
                    connecter.IsChecked = false;
                }
                else if (connecter.ConnecterType == ConnecterThumbTypesEnum.Line || LineStartThumb.ConnecterType == ConnecterThumbTypesEnum.Line)//one thumb is entity and the other is line
                {
                    LineConnecter lineConnecter;
                    EntityConnectingThumb entityConnecter;

                    if (connecter.ConnecterType == ConnecterThumbTypesEnum.Line)
                    {
                        lineConnecter = connecter as LineConnecter;
                        entityConnecter = LineStartThumb as EntityConnectingThumb;
                    }
                    else
                    {
                        lineConnecter = LineStartThumb as LineConnecter;
                        entityConnecter = connecter as EntityConnectingThumb;
                    }

                    Action<object[]> undoAction = x => { };
                    var undoParams = new object[3] { lineConnecter, entityConnecter, null };
                    Action<object[]> redoAction = x => { };
                    var redoParams = new object[3] { lineConnecter, entityConnecter, null };
                    if (lineConnecter.ConnecterIndex == 0)
                    {
                        undoParams[2] = lineConnecter._Connection.StartThumb;
                        
                        lineConnecter._Connection.StartThumb = entityConnecter;
                        redoAction += x => ((LineConnecter)x[0])._Connection.StartThumb = (EntityConnectingThumb)x[1];
                        undoAction += x => ((LineConnecter)x[0])._Connection.StartThumb = (EntityConnectingThumb)x[2];
                    }
                    else
                    {
                        undoParams[2] = lineConnecter._Connection.EndThumb;

                        lineConnecter._Connection.EndThumb = entityConnecter;
                        redoAction += x => ((LineConnecter)x[0])._Connection.EndThumb = (EntityConnectingThumb)x[1];
                        undoAction += x => ((LineConnecter)x[0])._Connection.EndThumb = (EntityConnectingThumb)x[2];
                    }

                    lineConnecter.IsChecked = false;
                    entityConnecter.IsChecked = false;
                    UndoRedoSystem.UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.LineConnection_ReconnectLine, undoAction, undoParams, redoAction, redoParams);
                }
                else //both thumbs are entity => new line
                {
                    var start = (EntityConnectingThumb)LineStartThumb;
                    var end = (EntityConnectingThumb)connecter;
                    end.CreateNewLineAsLineEndThumb(start);
                    end.IsChecked = false;
                    start.IsChecked = false;
                    //undo redo command in Lineconnection constructor
                }

                LineStartThumb = null;
            }
        }
        //internal void LineConnecterActionCancelledByStartThumb()
        //{
        //    LineStartThumb = null;
        //}
        #endregion
        
        #region lines
        public List<LineConnectionSaveProxy> GetConnectionsSerializationProxies()
        {
            var list = new List<LineConnectionSaveProxy>();
            foreach (var item in ChartCanvas.Children)
            {
                var connection = item as LineConnection;
                if (connection == null)
                    continue;

                list.Add(connection.GetSerializationProxy());
            }
            return list;
        }
        public List<LineConnection> AddLineConnectionsByProxiesAndReturn(IEnumerable<LineConnectionSaveProxy> connectionSaveProxies)
        {
            var result = new List<LineConnection>();
            var entitiesVMById = EntitiesViewModels
                .ToDictionary(x => x.ViewModelId, x => x);
            var textsVMById = VisualsTextsViewModels
                .ToDictionary(x => x.ViewModelId, x => x);

            EntityConnectingThumb startTh = null;
            EntityConnectingThumb endTH = null;
            EntityBorderUserControl border = null;
            IVisualText text = null;
            foreach (var item in connectionSaveProxies)
            {
                if (entitiesVMById.ContainsKey(item.StartVM))
                {
                    border = (EntityBorderUserControl)entitiesVMById[item.StartVM].UserControl;
                    startTh = (EntityConnectingThumb)border.GetThumbByType(item.StartThumb);
                }
                else if (textsVMById.ContainsKey(item.StartVM))
                {
                    text = textsVMById[item.StartVM].UserControl;
                    startTh = (EntityConnectingThumb)text.GetThumbByType(item.StartThumb);
                }
                else
                    continue;
                if (entitiesVMById.ContainsKey(item.EndVM))
                {
                    border = (EntityBorderUserControl)entitiesVMById[item.EndVM].UserControl;
                    endTH = (EntityConnectingThumb)border.GetThumbByType(item.EndThumb);
                }
                else if (textsVMById.ContainsKey(item.EndVM))
                {
                    text = textsVMById[item.EndVM].UserControl;
                    endTH = (EntityConnectingThumb)text.GetThumbByType(item.EndThumb);
                }
                else
                    continue;

                result.Add(new LineConnection(startTh, endTH, item.Segments));
            }

            return result;
        }
        public void AddLineConnectionsByProxies(IEnumerable<LineConnectionSaveProxy> connectionSaveProxies)
        {
            var entitiesVMById = EntitiesViewModels
                .ToDictionary(x => x.ViewModelId, x => x);
            var textsVMById = VisualsTextsViewModels
                .ToDictionary(x => x.ViewModelId, x => x);

            EntityConnectingThumb startTh = null;
            EntityConnectingThumb endTH = null;
            EntityBorderUserControl border = null;
            IVisualText text = null;
            foreach (var item in connectionSaveProxies)
            {
                if (entitiesVMById.ContainsKey(item.StartVM))
                {
                    border = (EntityBorderUserControl)entitiesVMById[item.StartVM].UserControl;
                    startTh = (EntityConnectingThumb)border.GetThumbByType(item.StartThumb);
                }
                else if (textsVMById.ContainsKey(item.StartVM))
                {
                    text = textsVMById[item.StartVM].UserControl;
                    startTh = (EntityConnectingThumb)text.GetThumbByType(item.StartThumb);
                }
                else
                    continue;
                if (entitiesVMById.ContainsKey(item.EndVM))
                {
                    border = (EntityBorderUserControl)entitiesVMById[item.EndVM].UserControl;
                    endTH = (EntityConnectingThumb)border.GetThumbByType(item.EndThumb);
                }
                else if (textsVMById.ContainsKey(item.EndVM))
                {
                    text = textsVMById[item.EndVM].UserControl;
                    endTH = (EntityConnectingThumb)text.GetThumbByType(item.EndThumb);
                }
                else
                    continue;

                new LineConnection(startTh, endTH, item.Segments);
            }
        }
        public void AddLineConnectionOnLoadingFile(IEnumerable<LineConnectionSaveProxy> connectionSaveProxies)
        {
            if(!LineConnection._LoadingFile)
            {
                lock(_LockObject)
                {
                    if (!LineConnection._LoadingFile)
                    {
                        LineConnection._LoadingFile = true;
                    }
                }
            }

            var entitiesVMById = EntitiesViewModels
                .ToDictionary(x => x.ViewModelId, x => x);
            var textsVMById = VisualsTextsViewModels
                .ToDictionary(x => x.ViewModelId, x => x);

            EntityConnectingThumb startTh = null;
            EntityConnectingThumb endTH = null;
            EntityBorderUserControl border = null;
            IVisualText text = null;
            foreach (var item in connectionSaveProxies)
            {
                if (entitiesVMById.ContainsKey(item.StartVM))
                {
                    border = (EntityBorderUserControl)entitiesVMById[item.StartVM].UserControl;
                    startTh = (EntityConnectingThumb)border.GetThumbByType(item.StartThumb);
                }
                else if (textsVMById.ContainsKey(item.StartVM))
                {
                    text = textsVMById[item.StartVM].UserControl;
                    startTh = (EntityConnectingThumb)text.GetThumbByType(item.StartThumb);
                }
                else
                    continue;
                if (entitiesVMById.ContainsKey(item.EndVM))
                {
                    border = (EntityBorderUserControl)entitiesVMById[item.EndVM].UserControl;
                    endTH = (EntityConnectingThumb)border.GetThumbByType(item.EndThumb);
                }
                else if (textsVMById.ContainsKey(item.EndVM))
                {
                    text = textsVMById[item.EndVM].UserControl;
                    endTH = (EntityConnectingThumb)text.GetThumbByType(item.EndThumb);
                }
                else
                    continue;

                new LineConnection(startTh, endTH, item.Segments);
            }

            if (LineConnection._LoadingFile)
            {
                lock (_LockObject)
                {
                    if (LineConnection._LoadingFile)
                    {
                        LineConnection._LoadingFile = false;
                    }
                }
            }
        }
        /// <summary>
        /// Don't use this for entities
        /// </summary>
        /// <param name="control"></param>
        /// <param name="fromLeft"></param>
        /// <param name="fromTop"></param>
        internal void AddElementInCoordinates(UIElement control, double fromLeft, double fromTop)
        {
            ChartCanvas.Children.Add(control);
            Canvas.SetLeft(control, fromLeft);
            Canvas.SetTop(control, fromTop);
        }
        /// <summary>
        /// Don't use this for entities
        /// </summary>
        /// <param name="control"></param>
        /// <param name="fromLeft"></param>
        /// <param name="fromTop"></param>
        public void MoveElementToCoordinates(UIElement control, double fromLeft, double fromTop)
        {
            Canvas.SetLeft(control, fromLeft);
            Canvas.SetTop(control, fromTop);
        }
        #endregion
        #endregion

        #region zoom and pan
        #region fields
        private readonly double _ScaleMin = 0.01d;
        private readonly double _ZoomSpeed = 0.2d;
        private readonly double _TransformOriginSpeed = 0.2d;
        private Point _PointOnClick;
        private Point _Origin;
        private ScaleTransform _ScaleTransform;
        private TranslateTransform _TranslateTransform;
        private TransformGroup _TransformGroup;
        private MouseHandlingMode _MouseHandlingMode = MouseHandlingMode.None;
        internal int _MouseEnterOnVisual;
        #endregion

        public double Scale { get; private set; }

        #region helpers
        private Point ClipMousePositionToCanvas(Point position)
        {
            double x = position.X;
            double y = position.Y;
            if (x < 0)
                x = 0;
            if (x > ChartCanvas.ActualWidth)
                x = ChartCanvas.ActualWidth;
            if (y < 0)
                y = 0;
            if (y > ChartCanvas.ActualHeight)
                y = ChartCanvas.ActualHeight;

            return new Point(x, y);
        }
        private double ClipXPositionToCanvas(double x)
        {
            if (x < 0)
                x = 0;
            if (x > ChartCanvas.ActualWidth)
                x = ChartCanvas.ActualWidth;
            return x;
        }
        private double ClipYPositionToCanvas(double y)
        {
            if (y < 0)
                y = 0;
            if (y > ChartCanvas.ActualHeight)
                y = ChartCanvas.ActualHeight;
            return y;
        }
        private double ClipTo01(double x)
        {
            if (x < 0)
                x = 0;
            if (x > 1)
                x = 1;
            return x;
        }
        #endregion

        #region animations
        /// <summary>
        /// Starts an animation to a particular value on the specified dependency property.
        /// You can pass in an event handler to call when the animation has completed.
        /// </summary>
        internal void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty, double toValue, double animationDurationSeconds, EventHandler completedEvent)
        {
            double fromValue = (double)animatableElement.GetValue(dependencyProperty);

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = fromValue;
            animation.To = toValue;
            animation.Duration = TimeSpan.FromSeconds(animationDurationSeconds);

            animation.Completed += delegate (object sender, EventArgs e)
            {
                //
                // When the animation has completed bake final value of the animation
                // into the property.
                //
                animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
                CancelAnimation(animatableElement, dependencyProperty);

                if (completedEvent != null)
                {
                    completedEvent(sender, e);
                }
            };

            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation);
        }
        internal void StartAnimation(Transform animatableElement, DependencyProperty dependencyProperty, double toValue, double animationDurationSeconds, EventHandler completedEvent)
        {
            double fromValue = (double)animatableElement.GetValue(dependencyProperty);

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = fromValue;
            animation.To = toValue;
            animation.Duration = TimeSpan.FromSeconds(animationDurationSeconds);

            animation.Completed += delegate (object sender, EventArgs e)
            {
                //
                // When the animation has completed bake final value of the animation
                // into the property.
                //
                animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
                CancelAnimation(animatableElement, dependencyProperty);

                if (completedEvent != null)
                {
                    completedEvent(sender, e);
                }
            };

            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation);
        }
        /// <summary>
        /// Cancel any animations that are running on the specified dependency property.
        /// </summary>
        internal void CancelAnimation(UIElement animatableElement, DependencyProperty dependencyProperty)
        {
            animatableElement.BeginAnimation(dependencyProperty, null);
        }
        internal void CancelAnimation(Transform animatableElement, DependencyProperty dependencyProperty)
        {
            animatableElement.BeginAnimation(dependencyProperty, null);
        }
        #endregion

        private void Zoom(Point point, double scale)
        {
            if (scale < _ScaleTransform.ScaleX && scale <= _ScaleMin)
                return;

            Scale = scale;
            double centerX = (point.X - _TranslateTransform.X) / _ScaleTransform.ScaleX;
            double centerY = (point.Y - _TranslateTransform.Y) / _ScaleTransform.ScaleY;

            double animationsDuration = _ZoomSpeed * 1.5d;
            StartAnimation(_ScaleTransform, ScaleTransform.ScaleXProperty, scale, animationsDuration, null);
            StartAnimation(_ScaleTransform, ScaleTransform.ScaleYProperty, scale, animationsDuration, null);

            var originX = ClipTo01(ChartCanvas.RenderTransformOrigin.X - ((ChartCanvas.RenderTransformOrigin.X - (point.X / ChartCanvas.ActualWidth)) * _TransformOriginSpeed * scale));
            var originY = ClipTo01(ChartCanvas.RenderTransformOrigin.Y - ((ChartCanvas.RenderTransformOrigin.Y - (point.Y / ChartCanvas.ActualHeight)) * _TransformOriginSpeed * scale));
            _Origin = new Point(originX, originY);
            ChartCanvas.RenderTransformOrigin = _Origin;
            StartAnimation(_TranslateTransform, TranslateTransform.XProperty, -(point.X - centerX) * _ScaleTransform.ScaleX * _ZoomSpeed, animationsDuration, null);
            StartAnimation(_TranslateTransform, TranslateTransform.YProperty, -(point.Y - centerY) * _ScaleTransform.ScaleY * _ZoomSpeed, animationsDuration, null);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            ChartItemsSelectionHandler.ClearItemsSelected();
            var mainVM = (IChartMainVM)DataContext;
            if (!mainVM.AddingEmptyEntity)
            {
                CaptureMouse();
                //Store click position relation to Parent of the canvas
                _PointOnClick = e.GetPosition(ChartCanvas);

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
                else if (e.ChangedButton == MouseButton.Middle)
                {
                    // Just a plain old left-down initiates panning mode.
                    _MouseHandlingMode = MouseHandlingMode.Panning;
                }
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            var mainVM = (IChartMainVM)DataContext;
            if (!mainVM.AddingEmptyEntity && !mainVM.AddingTextEntity && !mainVM.AddingSingleText && !mainVM.AddingEmptyLastTypeEntity && !mainVM.PasteIsChecked &&
                _MouseHandlingMode != MouseHandlingMode.None)
            {
                if (_MouseHandlingMode == MouseHandlingMode.DragSelecting)
                {
                    ChartItemsSelectionHandler.ApplyDragSelectionBox();
                }

                _MouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
            else if (mainVM.AddingEmptyEntity || mainVM.AddingTextEntity || mainVM.AddingSingleText || mainVM.AddingEmptyLastTypeEntity || mainVM.PasteIsChecked)
            {
                var p = e.GetPosition(ChartCanvas);
                mainVM.MouseUpWhileAddingEntity(e.GetPosition(ChartCanvas));
                ChartItemsSelectionHandler.ClearItemsSelected();
            }
            else
            {
                ChartItemsSelectionHandler.ClearItemsSelected();
            }
            ReleaseMouseCapture();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!ShowAllHiddableThumbs && _MouseEnterOnVisual == 0)
                HideAllThumbsIfAny();
            if (_MouseHandlingMode == MouseHandlingMode.Panning)
            {
                if (!IsMouseCaptured)
                    return;

                Point pointOnMove = e.GetPosition(ChartCanvas);

                _TranslateTransform.X = ChartCanvas.RenderTransform.Value.OffsetX - (_PointOnClick.X - pointOnMove.X) * _ScaleTransform.ScaleX;
                _TranslateTransform.Y = ChartCanvas.RenderTransform.Value.OffsetY - (_PointOnClick.Y - pointOnMove.Y) * _ScaleTransform.ScaleY;

                _PointOnClick = e.GetPosition(ChartCanvas);
            }
            else if (_MouseHandlingMode == MouseHandlingMode.Zooming || _MouseHandlingMode == MouseHandlingMode.Selecting)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point curContentMousePoint = e.GetPosition(ChartCanvas);

                    _MouseHandlingMode = MouseHandlingMode.DragSelecting;
                    ChartItemsSelectionHandler.InitDragSelectionBox(_PointOnClick, curContentMousePoint);
                }

                e.Handled = true;
            }
            else if (_MouseHandlingMode == MouseHandlingMode.DragSelecting)
            {
                Point curContentMousePoint = e.GetPosition(ChartCanvas);
                ChartItemsSelectionHandler.SetDragSelectionBox(_PointOnClick, curContentMousePoint);

                e.Handled = true;
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && _ScaleTransform.ScaleX <= _ZoomSpeed)
                return;
            Point mousePosition = ClipMousePositionToCanvas(e.GetPosition(ChartCanvas));

            double zoomNow = ChartCanvas.RenderTransform.Value.M11;
            double valZoom = e.Delta > 0 ? _ZoomSpeed : -_ZoomSpeed;
            Zoom(mousePosition, zoomNow + valZoom);
        }
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var canvasPosition = e.GetPosition(ChartCanvas);
            var center = TranslatePoint(
                new Point(ActualWidth * 0.5d, ActualHeight * 0.5d),
                ChartCanvas);

            var distance = canvasPosition - center;

            _TranslateTransform.X = _TranslateTransform.X - (distance.X * _ScaleTransform.ScaleX);
            _TranslateTransform.Y = _TranslateTransform.Y - (distance.Y * _ScaleTransform.ScaleY);
        }

        public void ZoomToFillScreen()
        {
            var x = (ActualWidth - 10) / ChartCanvas.ActualWidth;
            var y = (ActualHeight - 10) / ChartCanvas.ActualHeight;

            var min = Math.Min(x, y);

            if (min == _ScaleTransform.ScaleX)
                return;

            _Origin = new Point(0.5, 0.5);
            ChartCanvas.RenderTransformOrigin = _Origin;
            _ScaleTransform.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                new DoubleAnimation()
                {
                    From = _ScaleTransform.ScaleX,
                    To = min,
                    Duration = TimeSpan.FromMilliseconds(100)
                });
            _ScaleTransform.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                new DoubleAnimation()
                {
                    From = _ScaleTransform.ScaleY,
                    To = min,
                    Duration = TimeSpan.FromMilliseconds(100)
                });
            //_ScaleTransform.ScaleX = min;
            //_ScaleTransform.ScaleY = min;
        }
        public Tuple<List<double>, Point> GetCurrentZoom()
        {
            return new Tuple<List<double>, Point>(new List<double>(2) { _ScaleTransform.ScaleX, _ScaleTransform.ScaleY }, ChartCanvas.RenderTransformOrigin);
        }
        public void ForceSetZoom(List<double> scale, Point origin)
        {
            Scale = scale[0];
            _Origin = origin;
            ChartCanvas.RenderTransformOrigin = origin;
            _ScaleTransform.ScaleX = scale[0];
            _ScaleTransform.ScaleY = scale[1];
        }
        public void ResetZoom()
        {
            Zoom(GetCanvasCenter(), 1);
        }
        public void ApplyTransforms()
        {
            ChartCanvas.RenderTransform = _TransformGroup;
            ChartCanvas.RenderTransformOrigin = _Origin;
        }
        public void ClearTransforms()
        {
            var translateTransform = new TranslateTransform();
            var scaleTransform = new ScaleTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(_ScaleTransform);
            transformGroup.Children.Add(_TranslateTransform);

            ChartCanvas.RenderTransform = transformGroup;
            ChartCanvas.RenderTransformOrigin = new Point(0.5d, 0.5d);
            translateTransform.X = 0;
            translateTransform.Y = 0;
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;
        }
        #endregion
    }
}

#region old
//#region zoom box
///// <summary>
///// Initialise the rectangle that the use is dragging out.
///// </summary>
//public void InitDragZoomRect(Point pt1, Point pt2)
//{
//    SetDragZoomRect(pt1, pt2);

//    _ZoomBoxCanvas.Visibility = Visibility.Visible;
//    _ZoomBox.Opacity = 0.5;
//}
///// <summary>
///// Update the position and size of the rectangle that user is dragging out.
///// </summary>
//public void SetDragZoomRect(Point pt1, Point pt2)
//{
//    var data = new DragBoxesData(pt1, pt2);

//    //
//    // Update the coordinates of the rectangle that is being dragged out by the user.
//    // The we offset and rescale to convert from content coordinates.
//    //
//    Canvas.SetLeft(_ZoomBox, data.x);
//    Canvas.SetTop(_ZoomBox, data.y);
//    _ZoomBox.Width = data.width;
//    _ZoomBox.Height = data.height;
//}
///// <summary>
///// When the user has finished dragging out the rectangle the zoom operation is applied.
///// </summary>
//public void ApplyDragZoomRect()
//{
//    //
//    // Retreive the rectangle that the user draggged out and zoom in on it.
//    //
//    double contentX = Canvas.GetLeft(_ZoomBox);
//    double contentY = Canvas.GetTop(_ZoomBox);
//    double contentWidth = _ZoomBox.Width;
//    double contentHeight = _ZoomBox.Height;
//    ZoomAndPan.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

//    FadeOutDragZoomRect();
//}
///// <summary>
///// Fade out the drag zoom rectangle.
///// </summary>
//public void FadeOutDragZoomRect()
//{
//    AnimationHelper.StartAnimation(_ZoomBox, Border.OpacityProperty, 0.0, 0.1,
//        delegate (object sender, EventArgs e)
//        {
//            _ZoomBoxCanvas.Visibility = Visibility.Collapsed;
//        });
//}
//#endregion
/*
// * #region zoom and pan
//        //https://www.codeproject.com/Articles/85603/A-WPF-custom-control-for-zooming-and-panning

//        #region fields
//        //private double _ZoomSpeed = 0.01d;
//        /// <summary>
//        /// Specifies the current state of the mouse handling logic.
//        /// </summary>
//        //private MouseHandlingMode _MouseHandlingMode = MouseHandlingMode.None;
//        /// <summary>
//        /// The point that was clicked relative to the ZoomAndPanControl.
//        /// </summary>
//        private Point _OrigZoomAndPanControlMouseDownPoint;
//        /// <summary>
//        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
//        /// </summary>
//        private Point _OrigContentMouseDownPoint;
//        /// <summary>
//        /// Records which mouse button clicked during mouse dragging.
//        /// </summary>
//        private MouseButton _MouseButtonDown;
//        /// <summary>
//        /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
//        /// </summary>
//        private Rect _PrevZoomRect;
//        /// <summary>
//        /// Save the previous content scale, pressing the backspace key jumps back to this scale.
//        /// </summary>
//        private double _PrevZoomScale;
//        /// <summary>
//        /// Set to 'true' when the previous zoom rect is saved.
//        /// </summary>
//        private bool _PrevZoomRectSet = false;
//        #endregion

//        private void HandleKeyDown(Key key)
//        {
//            /*
//             * <Window.InputBindings>
//        <!--
//        Bind keys to commands.
//        -->
//<!--
//<KeyBinding Key="Minus" Command="{StaticResource Commands.ZoomOut}"/>
//<KeyBinding Key="Plus" Command="{StaticResource Commands.ZoomIn}"/>
//<KeyBinding Key="Backspace" Command="{StaticResource Commands.JumpBackToPrevZoom}"/>
//</Window.InputBindings>
//<Window.CommandBindings>
//    <!--
//        Bind commands to event handlers.
//        -->
//<!--
//    <CommandBinding Command="{StaticResource Commands.ZoomOut}" Executed="ZoomOut_Executed"/>
//    <CommandBinding Command="{StaticResource Commands.ZoomIn}" Executed="ZoomIn_Executed"/>
//    <CommandBinding Command="{StaticResource Commands.JumpBackToPrevZoom}" Executed="JumpBackToPrevZoom_Executed" CanExecute="JumpBackToPrevZoom_CanExecuted"/>
//    <CommandBinding Command="{StaticResource Commands.Fill}" Executed="Fill_Executed" />
//    <CommandBinding Command="{StaticResource Commands.OneHundredPercent}" Executed="OneHundredPercent_Executed"/>
//</Window.CommandBindings>
//                */
//            switch (key)
//            {
//                case Key.OemMinus:
//                case Key.Subtract:
//                    ZoomOut_Executed();
//                    break;
//                case Key.OemPlus:
//                case Key.Add:
//                    ZoomIn_Executed();
//                    break;
//                case Key.Back:
//                    //if (JumpBackToPrevZoom_CanExecuted())
//                    //    JumpBackToPrevZoom();
//                    //break;
//                case Key.Delete:
//                    RemoveSelectedItems();
//                    break;
//                case Key.Escape:
//                    CancellableActionsHandlerClass.Instance.CancelAllOnGoingActions(x => x.Id.Contains("CCC"));
//                    break;
//            }
//        }
//        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
//{
//    HandleKeyDown(e.Key);
//}
//protected override void OnPreviewKeyDown(KeyEventArgs e)
//{
//    HandleKeyDown(e.Key);
//    base.OnPreviewKeyDown(e);
//}

///// <summary>
///// Expand the content area to fit the rectangles.
///// </summary>
//public void ExpandContent()
//{
//    //double xOffset = 0;
//    //double yOffset = 0;
//    //Rect contentRect = new Rect(0, 0, 0, 0);
//    //foreach (RectangleData rectangleData in ZoomAndPanDataModel.Instance.Rectangles)
//    //{
//    //    if (rectangleData.X < xOffset)
//    //    {
//    //        xOffset = rectangleData.X;
//    //    }

//    //    if (rectangleData.Y < yOffset)
//    //    {
//    //        yOffset = rectangleData.Y;
//    //    }

//    //    contentRect.Union(new Rect(rectangleData.X, rectangleData.Y, rectangleData.Width, rectangleData.Height));
//    //}

//    ////
//    //// Translate all rectangles so they are in positive space.
//    ////
//    //xOffset = Math.Abs(xOffset);
//    //yOffset = Math.Abs(yOffset);

//    //foreach (RectangleData rectangleData in ZoomAndPanDataModel.Instance.Rectangles)
//    //{
//    //    rectangleData.X += xOffset;
//    //    rectangleData.Y += yOffset;
//    //}

//    //ZoomAndPanDataModel.Instance.ContentWidth = contentRect.Width;
//    //ZoomAndPanDataModel.Instance.ContentHeight = contentRect.Height;
//    var parent = (FrameworkElement)VisualTreeHelper.GetParent(this);
//    ZoomAndPanDataModel.Instance.ContentWidth = parent.ActualWidth;
//    ZoomAndPanDataModel.Instance.ContentHeight = parent.ActualHeight;
//}
///// <summary>
///// Event raised on mouse down in the ZoomAndPanControl.
///// </summary>
//private void zoomAndPanControl_MouseDown(object sender, MouseButtonEventArgs e)
//{
//    //TV.Focus();
//    //Keyboard.Focus(TV);
//    ChartItemsSelectionHandler.ClearItemsSelected();
//    var mainVM = (IChartMainVM)DataContext;
//    if (!mainVM.AddingEmptyEntity)
//    {
//        _MouseButtonDown = e.ChangedButton;
//        _OrigZoomAndPanControlMouseDownPoint = e.GetPosition(ZoomAndPan);
//        _OrigContentMouseDownPoint = e.GetPosition(ChartCanvas);

//        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
//            (e.ChangedButton == MouseButton.Left ||
//             e.ChangedButton == MouseButton.Right))
//        {
//            _MouseHandlingMode = MouseHandlingMode.Selecting;
//        }
//        else if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
//            (e.ChangedButton == MouseButton.Left ||
//             e.ChangedButton == MouseButton.Right))
//        {
//            // Control + left- or right-down initiates zooming mode.
//            _MouseHandlingMode = MouseHandlingMode.Zooming;
//        }
//        else if (_MouseButtonDown == MouseButton.Middle)
//        {
//            // Just a plain old left-down initiates panning mode.
//            _MouseHandlingMode = MouseHandlingMode.Panning;
//        }

//        if (_MouseHandlingMode != MouseHandlingMode.None)
//        {
//            // Capture the mouse so that we eventually receive the mouse up event.
//            CaptureMouse();
//        }
//    }
//}
///// <summary>
///// Event raised on mouse up in the ZoomAndPanControl.
///// </summary>
//private void zoomAndPanControl_MouseUp(object sender, MouseButtonEventArgs e)
//{
//    var mainVM = (IChartMainVM)DataContext;
//    if (!mainVM.AddingEmptyEntity && _MouseHandlingMode != MouseHandlingMode.None)
//    {
//        if (_MouseHandlingMode == MouseHandlingMode.Zooming)
//        {
//            if (_MouseButtonDown == MouseButton.Left)
//            {
//                // Shift + left-click zooms in on the content.
//                ZoomIn(_OrigContentMouseDownPoint);
//            }
//            else if (_MouseButtonDown == MouseButton.Right)
//            {
//                // Shift + left-click zooms out from the content.
//                ZoomOut(_OrigContentMouseDownPoint);
//            }
//        }
//        else if (_MouseHandlingMode == MouseHandlingMode.DragZooming)
//        {
//            // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.

//            //
//            // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
//            //
//            SavePrevZoomRect();

//            ApplyDragZoomRect();
//        }
//        else if (_MouseHandlingMode == MouseHandlingMode.DragSelecting)
//        {
//            ChartItemsSelectionHandler.ApplyDragSelectionBox();
//        }

//        ReleaseMouseCapture();
//        _MouseHandlingMode = MouseHandlingMode.None;
//        e.Handled = true;
//    }
//    else if (mainVM.AddingEmptyEntity || mainVM.AddingEmptyLastTypeEntity)
//    {
//        var p = e.GetPosition(ChartCanvas);
//        mainVM.MouseUpWhileAddingEntity(e.GetPosition(ChartCanvas));
//    }
//}
///// <summary>
///// Event raised on mouse move in the ZoomAndPanControl.
///// </summary>
//private void zoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
//{
//    HideEntityThumbsIfAny();
//    HideLineThumbsIfAny();
//    if (_MouseHandlingMode == MouseHandlingMode.Panning)
//    {
//        //
//        // The user is left-dragging the mouse.
//        // Pan the viewport by the appropriate amount.
//        //
//        Point curContentMousePoint = e.GetPosition(ChartCanvas);
//        Vector dragOffset = curContentMousePoint - _OrigContentMouseDownPoint;

//        ZoomAndPan.ContentOffsetX -= dragOffset.X;
//        ZoomAndPan.ContentOffsetY -= dragOffset.Y;

//        e.Handled = true;
//    }
//    else if (_MouseHandlingMode == MouseHandlingMode.Zooming || _MouseHandlingMode == MouseHandlingMode.Selecting)
//    {
//        Point curZoomAndPanControlMousePoint = e.GetPosition(ZoomAndPan);
//        Vector dragOffset = curZoomAndPanControlMousePoint - _OrigZoomAndPanControlMouseDownPoint;
//        double dragThreshold = 10;
//        if (_MouseButtonDown == MouseButton.Left &&
//            (Math.Abs(dragOffset.X) > dragThreshold ||
//             Math.Abs(dragOffset.Y) > dragThreshold))
//        {
//            //
//            // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
//            // initiate drag zooming mode where the user can drag out a rectangle to select the area
//            // to zoom in on.
//            //
//            Point curContentMousePoint = e.GetPosition(ChartCanvas);

//            if (_MouseHandlingMode == MouseHandlingMode.Zooming)
//            {
//                _MouseHandlingMode = MouseHandlingMode.DragZooming;
//                InitDragZoomRect(_OrigContentMouseDownPoint, curContentMousePoint);
//            }
//            else
//            {
//                _MouseHandlingMode = MouseHandlingMode.DragSelecting;
//                ChartItemsSelectionHandler.InitDragSelectionBox(_OrigContentMouseDownPoint, curContentMousePoint);
//            }
//        }

//        e.Handled = true;
//    }
//    else if (_MouseHandlingMode == MouseHandlingMode.DragZooming)
//    {
//        //
//        // When in drag zooming mode continously update the position of the rectangle
//        // that the user is dragging out.
//        //
//        Point curContentMousePoint = e.GetPosition(ChartCanvas);
//        SetDragZoomRect(_OrigContentMouseDownPoint, curContentMousePoint);

//        e.Handled = true;
//    }
//    else if (_MouseHandlingMode == MouseHandlingMode.DragSelecting)
//    {
//        Point curContentMousePoint = e.GetPosition(ChartCanvas);
//        ChartItemsSelectionHandler.SetDragSelectionBox(_OrigContentMouseDownPoint, curContentMousePoint);

//        e.Handled = true;
//    }
//}
///// <summary>
///// Event raised by rotating the mouse wheel
///// </summary>
//private void zoomAndPanControl_MouseWheel(object sender, MouseWheelEventArgs e)
//{
//    e.Handled = true;

//    if (e.Delta > 0)
//    {
//        Point curContentMousePoint = e.GetPosition(ZoomAndPan);
//        ZoomIn(curContentMousePoint);
//    }
//    else if (e.Delta < 0)
//    {
//        Point curContentMousePoint = e.GetPosition(ZoomAndPan);
//        ZoomOut(curContentMousePoint);
//    }
//}
///// <summary>
///// The 'ZoomIn' command (bound to the plus key) was executed.
///// </summary>
//private void ZoomIn_Executed()
//{
//    ZoomIn(new Point(ZoomAndPan.ContentZoomFocusX, ZoomAndPan.ContentZoomFocusY));
//}
///// <summary>
///// The 'ZoomOut' command (bound to the minus key) was executed.
///// </summary>
//private void ZoomOut_Executed()
//{
//    ZoomOut(new Point(ZoomAndPan.ContentZoomFocusX, ZoomAndPan.ContentZoomFocusY));
//}
///// <summary>
///// The 'JumpBackToPrevZoom' command was executed.
///// </summary>
//private void JumpBackToPrevZoom_Executed()
//{
//    JumpBackToPrevZoom();
//}
///// <summary>
///// Determines whether the 'JumpBackToPrevZoom' command can be executed.
///// </summary>
//private bool JumpBackToPrevZoom_CanExecuted()
//{
//    return _PrevZoomRectSet;
//}
///// <summary>
///// The 'Fill' command was executed.
///// </summary>
//private void Fill_Executed(object sender, ExecutedRoutedEventArgs e)
//{
//    SavePrevZoomRect();

//    ZoomAndPan.AnimatedScaleToFit();
//}
///// <summary>
///// The 'OneHundredPercent' command was executed.
///// </summary>
//private void OneHundredPercent_Executed(object sender, ExecutedRoutedEventArgs e)
//{
//    SavePrevZoomRect();

//    ZoomAndPan.AnimatedZoomTo(1.0);
//}
///// <summary>
///// Jump back to the previous zoom level.
///// </summary>
//private void JumpBackToPrevZoom()
//{
//    ZoomAndPan.AnimatedZoomTo(_PrevZoomScale, _PrevZoomRect);

//    ClearPrevZoomRect();
//}
///// <summary>
///// Zoom the viewport out, centering on the specified point (in content coordinates).
///// </summary>
//private void ZoomOut(Point contentZoomCenter)
//{
//    ZoomAndPan.ZoomAboutPoint(ZoomAndPan.ContentScale - _ZoomSpeed, contentZoomCenter);
//}
///// <summary>
///// Zoom the viewport in, centering on the specified point (in content coordinates).
///// </summary>
//private void ZoomIn(Point contentZoomCenter)
//{
//    ZoomAndPan.ZoomAboutPoint(ZoomAndPan.ContentScale + _ZoomSpeed, contentZoomCenter);
//}

///// <summary>
///// Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
///// </summary>
//private void SavePrevZoomRect()
//{
//    _PrevZoomRect = new Rect(ZoomAndPan.ContentOffsetX, ZoomAndPan.ContentOffsetY, ZoomAndPan.ContentViewportWidth, ZoomAndPan.ContentViewportHeight);
//    _PrevZoomScale = ZoomAndPan.ContentScale;
//    _PrevZoomRectSet = true;
//}
///// <summary>
///// Clear the memory of the previous zoom level.
///// </summary>
//private void ClearPrevZoomRect()
//{
//    _PrevZoomRectSet = false;
//}
///// <summary>
///// Event raised when the user has double clicked in the zoom and pan control.
///// </summary>
//private void zoomAndPanControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
//{
//    if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
//    {
//        Point doubleClickPoint = e.GetPosition(ChartCanvas);
//        ZoomAndPan.AnimatedSnapTo(doubleClickPoint);
//    }
//}
//        #endregion

#endregion
