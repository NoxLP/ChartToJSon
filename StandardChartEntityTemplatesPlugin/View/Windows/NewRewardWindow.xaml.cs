using StoryChart.ViewModel;
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
using System.Windows.Shapes;

namespace StoryChart.View.Windows
{
    /// <summary>
    /// Lógica de interacción para AddRewardWindow.xaml
    /// </summary>
    public partial class NewRewardWindow : Window
    {
        public NewRewardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((NewRewardWindowVM)DataContext).OnWindowLoaded(this);
        }
    }
}
