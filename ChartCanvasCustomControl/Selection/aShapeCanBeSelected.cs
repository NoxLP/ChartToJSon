using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ChartCanvasNamespace.Selection
{
    //Based on http://www.charlespetzold.com/blog/2007/04/191200.html
    public abstract class aShapeCanBeSelected : Shape, IVisualCanBeSelected
    {
        public CanBeSelectedItemTypeEnum Type => CanBeSelectedItemTypeEnum.LineConnection;
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(aShapeCanBeSelected), new PropertyMetadata(false, IsSelectedPropertyChanged));
        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((aShapeCanBeSelected)d).IsSelectedChanged((bool)e.NewValue);
        }
        private void IsSelectedChanged(bool value)
        {
            UpdateSelectedVisualEffect();
            //if (value)
            //    ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
        }

        internal bool _SelfSelecting;
        public ModifierKeys ModifiersWhenSelectingSelf { get; private set; }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            _SelfSelecting = true;
            ModifiersWhenSelectingSelf = Keyboard.Modifiers;

            var forget = Task.Run(async () =>
            {
                await Task.Delay(2000);
                if (_SelfSelecting)
                    _SelfSelecting = false;
            });
            base.OnPreviewMouseDown(e);
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (_SelfSelecting)
            {
                ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
                _SelfSelecting = false;
            }
            base.OnPreviewMouseUp(e);
        }
        protected abstract void UpdateSelectedVisualEffect();

        public bool Equals(IVisualCanBeSelected other)
        {
            return Equals(other as UIElement);
        }
    }
}
