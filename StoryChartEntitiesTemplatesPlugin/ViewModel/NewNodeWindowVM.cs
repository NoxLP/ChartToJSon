using StoryChart.Model;
using StoryChart.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFHelpers.Commands;
using WPFHelpers.ViewModelBase;

namespace StoryChart.ViewModel
{
    [Serializable]
    public class NewNodeWindowVM : aViewModelBase
    {
        public NewNodeWindowVM(Node newNode)
        {
            NewNodeVM = new NodeViewModel() { Node = newNode };
            OkCommand = new DelegateCommand(x => Ok(), null);
            CancelCommand = new DelegateCommand(x => Cancel(), null);
        }

        private NewNodeWindow _MyWindow;
        private NodeViewModel _NewNodeVM;

        public NodeViewModel NewNodeVM
        {
            get { return _NewNodeVM; }
            set
            {
                if (value == null)
                {
                    if (_NewNodeVM != null)
                    {
                        _NewNodeVM = null;
                        OnPropertyChanged(nameof(NewNodeVM));
                    }
                }
                else if (!value.Equals(_NewNodeVM))
                {
                    _NewNodeVM = value;
                    OnPropertyChanged(nameof(NewNodeVM));
                }
            }
        }

        public void OnWindowLoaded(NewNodeWindow myWindow)
        {
            _MyWindow = myWindow;
        }

        #region commands
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void Ok()
        {
            _MyWindow.DialogResult = true;
            _MyWindow.Close();
        }
        private void Cancel()
        {
            _MyWindow.DialogResult = false;
            _MyWindow.Close();
        }
        #endregion
    }
}
