using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChartCanvasNamespace.Selection
{
    //public abstract class aUIElementCanBeSelected : UIElement, IVisualCanBeSelected
    //{
    //    public bool IsSelected
    //    {
    //        get { return (bool)GetValue(IsSelectedProperty); }
    //        set { SetValue(IsSelectedProperty, value); }
    //    }
    //    public static readonly DependencyProperty IsSelectedProperty =
    //        DependencyProperty.Register("IsSelected", typeof(bool), typeof(aUIElementCanBeSelected), new PropertyMetadata(false, IsSelectedPropertyChanged));
    //    private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        ((aUIElementCanBeSelected)d).IsSelectedChanged((bool)e.NewValue);
    //    }
    //    private void IsSelectedChanged(bool value)
    //    {
    //        UpdateSelectedVisualEffect();
    //        //if (value)
    //        //    ChartCustomControl.Instance.ChartItemsSelectionHandler.ItemSelected(this);
    //    }

    //    private bool _SelectingThis;
    //    public ModifierKeys ModifiersWhenSelectingSelf { get; private set; }

    //    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    //    {
    //        _SelectingThis = true;
    //        ModifiersWhenSelectingSelf = Keyboard.Modifiers;

    //        var forget = Task.Run(async () =>
    //        {
    //            await Task.Delay(2000);
    //            if (_SelectingThis)
    //                _SelectingThis = false;
    //        });
    //        base.OnPreviewMouseDown(e);
    //    }
    //    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    //    {
    //        if (_SelectingThis)
    //        {
    //            _SelectingThis = false;
    //            IsSelected = true;
    //        }
    //        base.OnPreviewMouseUp(e);
    //    }
    //    protected abstract void UpdateSelectedVisualEffect();

    //    public bool Equals(IVisualCanBeSelected other)
    //    {
    //        return Equals(other as UIElement);
    //    }
    //}

    
}
