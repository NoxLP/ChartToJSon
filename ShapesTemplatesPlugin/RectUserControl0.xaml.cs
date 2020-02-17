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

namespace ShapesTemplatesPlugin
{
    /// <summary>
    /// Lógica de interacción para RectUserControl0.xaml
    /// </summary>
    public partial class RectUserControl0 : UserControl
    {
        public RectUserControl0()
        {
            InitializeComponent();
        }

        public Shape UserControlShape
        {
            get { return (Shape)GetValue(UserControlShapeProperty); }
            set { SetValue(UserControlShapeProperty, value); }
        }
        public static readonly DependencyProperty UserControlShapeProperty =
            DependencyProperty.Register("UserControlShape", typeof(Shape), typeof(RectUserControl0), new PropertyMetadata(null));
    }
}
