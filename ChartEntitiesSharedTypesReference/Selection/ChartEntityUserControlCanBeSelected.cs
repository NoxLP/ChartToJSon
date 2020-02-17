using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ChartCanvasNamespace.Selection
{
    public class ChartEntityUserControlCanBeSelected : UserControl, IVisualCanBeSelected
    {
        public ChartEntityUserControlCanBeSelected()
        {
            Loaded += ChartEntityUserControlCanBeSelected_Loaded;
        }

        public CanBeSelectedItemTypeEnum Type { get { return CanBeSelectedItemTypeEnum.EntityBorder; } }
        public IChartItemsSelectionHandler SelectionHandler { get; set; }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ChartEntityUserControlCanBeSelected), new PropertyMetadata(false, IsSelectedPropertyChanged));
        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChartEntityUserControlCanBeSelected)d).IsSelectedChanged((bool)e.NewValue);
        }
        private void IsSelectedChanged(bool value)
        {
            UpdateSelectedVisualEffect();
            var vm = DataContext as IChartEntityViewModel;
            if (vm == null)
                return;
            vm.IsSelected = value;
            //if (value && _SelectingThis)
            //    ChartControl.Instance.ItemSelected(this);
        }

        internal bool _SelfSelecting { get; private set; }
        public ModifierKeys ModifiersWhenSelectingSelf { get; private set; }

        private void ChartEntityUserControlCanBeSelected_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as IChartEntityViewModel;
            if (vm == null)
                return;

            var b = new Binding()
            {
                Source = DataContext,
                Path = new PropertyPath("IsSelected"),
                Mode = BindingMode.TwoWay
            };
            BindingOperations.SetBinding(this, IsSelectedProperty, b);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _SelfSelecting = true;
            ModifiersWhenSelectingSelf = Keyboard.Modifiers;

            var forget = Task.Run(async () =>
            {
                await Task.Delay(2000);
                if (_SelfSelecting)
                    _SelfSelecting = false;
            });
            base.OnMouseDown(e);
            e.Handled = true;
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if(_SelfSelecting)
            {
                //ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
                SelectionHandler.ItemSelected(this);
                _SelfSelecting = false;
            }
            base.OnMouseUp(e);
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 || (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _SelfSelecting = true;
                ModifiersWhenSelectingSelf = Keyboard.Modifiers;

                var forget = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    if (_SelfSelecting)
                        _SelfSelecting = false;
                });
                base.OnMouseDown(e);
                e.Handled = true;
            }
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (_SelfSelecting)
            {
                //ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
                SelectionHandler.ItemSelected(this);
                _SelfSelecting = false;
            }
            base.OnPreviewMouseUp(e);
        }
        protected virtual void UpdateSelectedVisualEffect() { }

        public bool Equals(IVisualCanBeSelected other)
        {
            return Equals(other as UserControl);
        }
    }
}
