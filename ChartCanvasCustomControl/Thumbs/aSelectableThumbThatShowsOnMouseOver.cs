using ChartCanvasNamespace.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ChartCanvasNamespace.Thumbs
{
    public abstract class aSelectableThumbThatShowsOnMouseOver : Thumb, IChartThumbThatShowsOnMouseOver, IVisualCanBeSelected
    {
        public CanBeSelectedItemTypeEnum Type => CanBeSelectedItemTypeEnum.Thumb;

        protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && Visibility != Visibility.Visible)
            {
                Visibility = Visibility.Visible;
                Mouse.OverrideCursor = Cursors.Hand;
            }
            else if (!(bool)e.NewValue && Visibility != Visibility.Hidden)
            {
                Visibility = Visibility.Hidden;
                Mouse.OverrideCursor = null;
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(aSelectableThumbThatShowsOnMouseOver), new PropertyMetadata(false, IsSelectedPropertyChanged));
        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((aSelectableThumbThatShowsOnMouseOver)d).IsSelectedChanged((bool)e.NewValue);
        }
        private void IsSelectedChanged(bool value)
        {
            UpdateSelectedVisualEffect();
            //if (value && _SelectingThis)
            //    ChartControl.Instance.ItemSelected(this);
        }

        public ModifierKeys ModifiersWhenSelectingSelf { get; private set; }
        
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            ModifiersWhenSelectingSelf = Keyboard.Modifiers;

            ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
            base.OnPreviewMouseDown(e);
        }
        protected abstract void UpdateSelectedVisualEffect();

        public bool Equals(IVisualCanBeSelected other)
        {
            return Equals(other as UIElement);
        }
    }
}
