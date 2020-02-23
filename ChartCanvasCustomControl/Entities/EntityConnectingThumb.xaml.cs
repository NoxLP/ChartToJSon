using ChartCanvasNamespace.Lines;
using ChartCanvasNamespace.Thumbs;
using ChartCanvasNamespace.VisualsBase;
//using ChartCanvasNamespace.ThumbsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UndoRedoSystem.UndoRedoCommands;
using WPFHelpers.CancelActions;

namespace ChartCanvasNamespace.Entities
{
    /// <summary>
    /// Lógica de interacción para EntityConnectingThumb.xaml
    /// </summary>
    public partial class EntityConnectingThumb : UserControl, IChartEntityConnectingThumb, IObjectWithCancellableAction, IChartObjectCanBeRemovedByParent, INotifyPropertyChanged, IEquatable<EntityConnectingThumb>
    {
        public EntityConnectingThumb()
        {
            InitializeComponent();
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_EntityConnectingThumb);
            ToolTip = Properties.ToolTips.Default.ToolTips_EntityConnectingThumb;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        internal List<LineConnection> _LinesStarts = new List<LineConnection>();
        internal List<LineConnection> _LinesEnds = new List<LineConnection>();

        public IViewModelOfVisualWithConnectingThumbs MyEntityViewModel
        {
            get { return (IViewModelOfVisualWithConnectingThumbs)GetValue(MyEntityViewModelProperty); }
            set { SetValue(MyEntityViewModelProperty, value); }
        }
        public static readonly DependencyProperty MyEntityViewModelProperty =
            DependencyProperty.Register("MyEntityViewModel", typeof(IViewModelOfVisualWithConnectingThumbs), typeof(EntityConnectingThumb), new PropertyMetadata(null));

        public EntityConnectingThumbTypesEnum Type
        {
            get { return (EntityConnectingThumbTypesEnum)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(EntityConnectingThumbTypesEnum), typeof(EntityConnectingThumb), new PropertyMetadata(EntityConnectingThumbTypesEnum.Top));

        public IVisualWithConnectingThumbs MyBorder
        {
            get { return (IVisualWithConnectingThumbs)GetValue(MyBorderProperty); }
            set { SetValue(MyBorderProperty, value); }
        }
        public static readonly DependencyProperty MyBorderProperty =
            DependencyProperty.Register("MyBorder", typeof(IVisualWithConnectingThumbs), typeof(EntityConnectingThumb), new PropertyMetadata(null));

        public Point AnchorPoint
        {
            get { return (Point)GetValue(AnchorPointProperty); }
            private set { SetValue(AnchorPointProperty, value); }
        }
        public static readonly DependencyProperty AnchorPointProperty =
            DependencyProperty.Register("AnchorPoint", typeof(Point), typeof(EntityConnectingThumb), new PropertyMetadata(default(Point)));

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAnchorPoint();
        }
        private void UpdateAnchorPoint()
        {
            if(_LinesStarts.Count > 0 || _LinesEnds.Count > 0)
                Console.WriteLine("Update anchor point");
            Size size = RenderSize;
            Point ofs = new Point(size.Width / 2, size.Height / 2); //isInput ? 0 : size.Height);
            if (_LinesStarts.Count > 0 || _LinesEnds.Count > 0)
                Console.WriteLine($"ofs = {ofs} {Environment.NewLine}Pre anchorpoint = {this.TranslatePoint(ofs, ChartCustomControl.Instance.ChartCanvas)}");
            AnchorPoint = this.TranslatePoint(ofs, ChartCustomControl.Instance.ChartCanvas); //TransformToVisual(MyCanvas).Transform(ofs);
            if (_LinesStarts.Count > 0 || _LinesEnds.Count > 0)
                Console.WriteLine($"AnchorPoint= {AnchorPoint}");
            //OnPropertyChanged(nameof(AnchorPoint));
            //OnPropertyChanged(nameof(AnchorPointProperty));
            //foreach (var item in _LinesStarts)
            //{
            //    item.DoAllSegments(s => s.UpdateStartEndBindings());
            //}
            //foreach (var item in _LinesEnds)
            //{
            //    item.DoAllSegments(s => s.UpdateStartEndBindings());
            //}
        }
        public void InitAnchorPoint()
        {
            double x = 0;
            double y = 0;
            switch (Name)
            {
                case "ThTop":
                    x = MyEntityViewModel.CanvasX;
                    y = MyEntityViewModel.CanvasY - (MyBorder.BorderContentHeight * 0.5d) - (MyBorder.BaseRootGrid.RowDefinitions[0].ActualHeight * 0.5d);
                    break;
                case "ThBottom":
                    x = MyEntityViewModel.CanvasX;
                    y = MyEntityViewModel.CanvasY + (MyBorder.BorderContentHeight * 0.5d) + (MyBorder.BaseRootGrid.RowDefinitions[0].ActualHeight * 0.5d);
                    break;
                case "ThLeft":
                    x = MyEntityViewModel.CanvasX - (MyBorder.BorderContentWidth * 0.5d) - (MyBorder.BaseRootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
                    y = MyEntityViewModel.CanvasY;
                    break;
                case "ThRight":
                    x = MyEntityViewModel.CanvasX + (MyBorder.BorderContentWidth * 0.5d) + (MyBorder.BaseRootGrid.ColumnDefinitions[0].ActualWidth * 0.5d);
                    y = MyEntityViewModel.CanvasY;
                    break;
            }

            AnchorPoint = new Point(x, y);
        }
        public void AutomaticMoveUpdateAnchorPoint()
        {
            double x = Canvas.GetLeft(MyBorder.GetUIElement);
            double y = Canvas.GetTop(MyBorder.GetUIElement);
            switch (Name)
            {
                case "ThTop":
                    x += (MyBorder.BorderContentWidth * 0.5d) + (MyBorder.BaseRootGrid.ColumnDefinitions[0].ActualWidth * 1.75d);
                    y += (MyBorder.BaseRootGrid.RowDefinitions[0].ActualHeight * 0.5d);
                    break;
                case "ThBottom":
                    x += (MyBorder.BorderContentWidth * 0.5d) + (MyBorder.BaseRootGrid.ColumnDefinitions[0].ActualWidth * 1.75d);
                    y += MyBorder.BorderContentHeight + (MyBorder.BaseRootGrid.RowDefinitions[0].ActualHeight * 1.5d);
                    break;
                case "ThLeft":
                    x += (MyBorder.BaseRootGrid.ColumnDefinitions[0].ActualWidth);
                    y += (MyBorder.BorderContentHeight * 0.5d) + (MyBorder.BaseRootGrid.RowDefinitions[0].ActualHeight);
                    break;
                case "ThRight":
                    x += (MyBorder.BaseRootGrid.ColumnDefinitions[1].ActualWidth) + (MyBorder.BaseRootGrid.ColumnDefinitions[0].ActualWidth * 2.25d);
                    y += (MyBorder.BorderContentHeight * 0.5d) + (MyBorder.BaseRootGrid.RowDefinitions[0].ActualHeight);
                    break;
            }
            AnchorPoint = new Point(x, y);
        }
        public void MyBorderMovedTo(bool automatic)
        {
            if (_LinesStarts.Count > 0 || _LinesEnds.Count > 0)
                Console.WriteLine("My border moved to");
            if (!automatic)
                UpdateAnchorPoint();
            else
                AutomaticMoveUpdateAnchorPoint();
            //foreach (var line in _LinesStarts)
            //{
            //    //line.Source = AnchorPoint;
            //    line.MoveLine();
            //}
            //foreach (var line in _LinesEnds)
            //{
            //    //line.Destination = AnchorPoint;
            //    line.MoveLine();
            //}
        }

        #region lines - connect entities
        public ConnecterThumbTypesEnum ConnecterType { get { return ConnecterThumbTypesEnum.Entity; } }
        public bool ScriptUnchecked { get; set; }

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(EntityConnectingThumb), new PropertyMetadata(false));

        //private PropertyPath GetPropertyPathByThumbName(string name)
        //{
        //    if (name.Contains("Left"))
        //        return new PropertyPath(EntityBorderUserControl.LeftProperty);
        //    if (name.Contains("Right"))
        //        return new PropertyPath(EntityBorderUserControl.RightProperty);
        //    if (name.Contains("Top"))
        //        return new PropertyPath(EntityBorderUserControl.TopProperty);
        //    //if (name.Contains("Bottom"))
        //    return new PropertyPath(EntityBorderUserControl.BottomProperty);
        //}
        public void SetNewLineCancellableAction()
        {
            CancelCurrentActionDelegate = CancelNewLine;
            CancellableActionsHandlerClass.Instance.NewCancellableAction(this);
        }
        public void CreateNewLineAsLineEndThumb(IChartEntityConnectingThumb startThumb)
        {
            var line = new LineConnection((EntityConnectingThumb)startThumb, this);
        }
        
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChartCustomControl.Instance.ConnecterChecked(this);
        }
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            if (ScriptUnchecked)
            {
                ScriptUnchecked = false;
                return;
            }

            CancellableActionsHandlerClass.Instance.CancellableActionFinished(this);
        }
        #endregion

        #region remove
        public bool AlreadyRemovedByParent { get; set; }
        public void AddToLastCommandMyUndoRedoCommands()
        {
            if (UndoRedoSystem.UndoRedoCommandManager.Instance.LastCommandIsNull)
                return;

            UndoRedoSystem.UndoRedoCommandManager.Instance.AddToLastCommand(null, null, x => RedoRemoveCommand(x), new object[1] { this });
            foreach (var line in _LinesStarts)
            {
                line.AddToLastCommandMyUndoRedoCommands();
            }
            foreach (var line in _LinesEnds)
            {
                line.AddToLastCommandMyUndoRedoCommands();
            }
        }
        public void RemovedByParent()
        {
            if (_LinesStarts.Count > 0)
            {
                var lines = new List<LineConnection>(_LinesStarts);
                foreach (var line in lines)
                {
                    line.RemovedByParent();
                }
            }
            if (_LinesEnds.Count > 0)
            {
                var lines = new List<LineConnection>(_LinesEnds);
                foreach (var line in lines)
                {
                    line.RemovedByParent();
                }
            }
        }
        private void RedoRemoveCommand(object[] parameters)
        {
            var ECT = parameters[0] as EntityConnectingThumb;
            foreach (var line in ECT._LinesStarts)
            {
                line.RemovedByParent();
            }
            foreach (var line in _LinesEnds)
            {
                line.RemovedByParent();
            }
        }
        #endregion

        #region cancel
        public string CancellableId => $"ChCuCo_ConnectingThumb_{Guid.NewGuid().ToString()}";
        public Action CancelCurrentActionDelegate { get; private set; }
        public Func<Task> AsyncCancelCurrentActionDelegate => null;

        internal void CancelNewLine()
        {
            ScriptUnchecked = true;
            this.IsChecked = false;
            CancelCurrentActionDelegate = null;
        }
        public bool Equals(IObjectWithCancellableAction other)
        {
            var obj = other as EntityConnectingThumb;
            if (obj == null)
                return false;
            return this.Equals(obj as UserControl);
        }
        #endregion

        public bool Equals(EntityConnectingThumb other)
        {
            bool borrame = other != null &&
                   base.Equals(other) &&
                   EqualityComparer<IViewModelOfVisualWithConnectingThumbs>.Default.Equals(MyEntityViewModel, other.MyEntityViewModel) &&
                   Type == other.Type &&
                   CancellableId == other.CancellableId;
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<IViewModelOfVisualWithConnectingThumbs>.Default.Equals(MyEntityViewModel, other.MyEntityViewModel) &&
                   Type == other.Type &&
                   CancellableId == other.CancellableId;
        }
        public new int GetHashCode()
        {
            var hashCode = -1858220299;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IViewModelOfVisualWithConnectingThumbs>.Default.GetHashCode(MyEntityViewModel);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CancellableId);
            return hashCode;
        }
    }
}
