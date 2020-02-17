using StoryChart.Model;
using StoryChart.Model.Rewards;
using StoryChart.Model.Triggers;
using StoryChart.View.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StoryChart.ViewModel
{
    public class NodeViewModel : Base.aViewModelBase, IEquatable<NodeViewModel>
    {
        #region Constructors
        public NodeViewModel()
        {
            AddRewardCommand = new DelegateCommand(x => AddReward(), null);
            AddTriggerCommand = new DelegateCommand(x => AddTrigger(), null);
        }
        #endregion Constructors

        #region Fields
        private readonly static Brush _BackgroundBrushUnselected = new SolidColorBrush(Colors.White);
        private readonly static Brush _BackgroundBrushSelected = new SolidColorBrush(Colors.AliceBlue);
        private bool _IsSelected;
        private Model.Node _Node;
        private ICollectionView _Rewards;
        private ICollectionView _Triggers;
        private Brush _Background;
        public NodeViewModel _MyParent;
        #endregion

        #region Properties
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    MessengerNSpace.Messenger.SendGuidedMessage(Properties.Settings.Default.MESSAGE_NODESELECTED, this.Node);
                    if (!value)
                        Background = _BackgroundBrushUnselected;
                    else
                        Background = _BackgroundBrushSelected;
                }
            }
        }
        public ICollectionView Rewards
        {
            get
            {
                var rewards = CollectionViewSource.GetDefaultView(Node.Rewards);
                rewards.SortDescriptions.Add(new SortDescription() { PropertyName = nameof(Model.Rewards.aNodeReward.Id) });

                return rewards;
            }
        }
        public ICollectionView Triggers
        {
            get
            {
                var triggers = CollectionViewSource.GetDefaultView(Node.Triggers);
                triggers.SortDescriptions.Add(new SortDescription() { PropertyName = nameof(Model.Triggers.aNodeTrigger.Type) });

                return triggers;
            }
        }
        public Model.Node Node
        {
            get { return _Node; }
            set
            {
                if (value == null)
                {
                    if (_Node != null)
                    {
                        _Node = null;
                        OnPropertyChanged(nameof(Node));
                    }
                }
                else if (!value.Equals(_Node))
                {
                    _Node = value;
                    OnPropertyChanged(nameof(Node));
                }
            }
        }
        public Brush Background
        {
            get { return _Background; }
            set
            {
                if (value == null)
                {
                    if (_Background != null)
                    {
                        _Background = null;
                        OnPropertyChanged(nameof(Background));
                    }
                }
                else if (!value.Equals(_Background))
                {
                    _Background = value;
                    OnPropertyChanged(nameof(Background));
                }
            }
        }
        #endregion

        #region commands
        private void AddReward()
        {
            var window = new NewRewardWindow();
            var result = window.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            Node.Rewards.Add((aNodeReward)((NewRewardWindowVM)window.DataContext).Reward);
            Rewards.Refresh();
            OnPropertyChanged(nameof(Rewards));
        }
        private void AddTrigger()
        {
            var window = new NewTriggerWindow();
            var result = window.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            Node.Triggers.Add((aNodeTrigger)((NewTriggerWindowVM)window.DataContext).Trigger);
            Triggers.Refresh();
            OnPropertyChanged(nameof(Triggers));
        }

        public ICommand AddRewardCommand { get; private set; }
        public ICommand AddTriggerCommand { get; private set; }
        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj as NodeViewModel);
        }
        public bool Equals(NodeViewModel other)
        {
            return other != null &&
                   EqualityComparer<Node>.Default.Equals(Node, other.Node);
        }
        public override int GetHashCode()
        {
            return -56134859 + EqualityComparer<Node>.Default.GetHashCode(Node);
        }
    }
}
