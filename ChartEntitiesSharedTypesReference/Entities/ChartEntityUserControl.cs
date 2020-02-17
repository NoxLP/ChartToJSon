using ChartCanvasNamespace.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChartCanvasNamespace.Entities.EntitiesShapesUserControls
{
    public class ChartEntityUserControl : ChartEntityUserControlCanBeSelected
    {
        public ChartEntityUserControl()
        {
            this.Loaded += EntityShapeUserControl_Loaded;
        }

        protected static SolidColorBrush _SelectedBorderBrush { get { return Brushes.Blue; } }

        private void EntityShapeUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var vm = DataContext as IChartEntityViewModel;
                if (vm == null)
                    throw new ChartEntitiesSharedTypesReference.Exceptions.IncorrectEntityUserControlDataContextTypeException();
                vm.UserControl.EntityUserControl = this;
            }
        }

        public virtual void ParentUpdatedSelectedVisualEffect(IVisualCanBeSelected selectedParent) { }
    }
}
