using StoryChart.Model.Rewards;
using StoryChart.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StoryChart.ViewModel
{
    public class NewRewardWindowVM : Base.aViewModelBase
    {
        public NewRewardWindowVM()
        {
            OkCommand = new DelegateCommand(x => Ok(), null);
            CancelCommand = new DelegateCommand(x => Cancel(), null);
        }

        #region Fields
        private NewRewardWindow _MyWindow;
        private INodeReward _Reward;
        private RewardsTypesEnum _Type;
        #endregion Fields

        #region Properties
        public INodeReward Reward
        {
            get { return _Reward; }
            set
            {
                if (value == null)
                {
                    if (_Reward != null)
                    {
                        _Reward = null;
                        OnPropertyChanged(nameof(Reward));
                    }
                }
                else if (!value.Equals(_Reward))
                {
                    _Reward = value;
                    OnPropertyChanged(nameof(Reward));
                }
            }
        }
        public RewardsTypesEnum Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    OnPropertyChanged(nameof(Type));
                    switch(value)
                    {
                        case RewardsTypesEnum.None:
                            Reward = null;
                            break;
                        case RewardsTypesEnum.Conversation:
                            Reward = new ConversationReward();
                            break;
                        case RewardsTypesEnum.Cinematic_CutScene:
                            Reward = new CinematicCutSceneReward();
                            break;
                        case RewardsTypesEnum.Item:
                            Reward = new ItemReward();
                            break;
                        case RewardsTypesEnum.Skill:
                            Reward = new SkillReward();
                            break;
                        case RewardsTypesEnum.Variable:
                            Reward = new VariableReward();
                            break;
                    }
                }
            }
        }
        #endregion Properties

        #region Methods
        public void OnWindowLoaded(NewRewardWindow myWindow)
        {
            _MyWindow = myWindow;
        }
        #endregion

        #region commands
        private void Cancel()
        {
            _MyWindow.DialogResult = false;
            _MyWindow.Close();
        }
        private void Ok()
        {
            _MyWindow.DialogResult = true;
            _MyWindow.Close();
        }
        public ICommand CancelCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        #endregion
    }
}
