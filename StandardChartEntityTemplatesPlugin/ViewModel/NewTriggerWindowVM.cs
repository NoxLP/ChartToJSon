using StoryChart.Model.Triggers;
using StoryChart.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StoryChart.ViewModel
{
    class NewTriggerWindowVM : Base.aViewModelBase
    {
        public NewTriggerWindowVM()
        {
            OkCommand = new DelegateCommand(x => Ok(), null);
            CancelCommand = new DelegateCommand(x => Cancel(), null);

        }

        #region Fields
        private NewTriggerWindow _MyWindow;
        private aNodeTrigger _Trigger;
        private TriggerTypesEnum _Type;
        #endregion Fields

        #region Properties
        public aNodeTrigger Trigger
        {
            get { return _Trigger; }
            set
            {
                if (value == null)
                {
                    if (_Trigger != null)
                    {
                        _Trigger = null;
                        OnPropertyChanged(nameof(Trigger));
                    }
                }
                else if (!value.Equals(_Trigger))
                {
                    _Trigger = value;
                    OnPropertyChanged(nameof(Trigger));
                }
            }
        }
        public TriggerTypesEnum Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    OnPropertyChanged(nameof(Type));
                    switch (value)
                    {
                        case TriggerTypesEnum.None:
                            Trigger = null;
                            break;
                        case TriggerTypesEnum.Conversation:
                            Trigger = new ConversationTrigger();
                            break;
                        case TriggerTypesEnum.ItemGrab:
                            Trigger = new ItemGrabTrigger();
                            break;
                        case TriggerTypesEnum.ItemInteraction:
                            Trigger = new ItemInteractionTrigger();
                            break;
                        case TriggerTypesEnum.Location:
                            Trigger = new LocationTrigger();
                            break;
                        case TriggerTypesEnum.MonsterDefeat:
                            Trigger = new MonsterDefeatTrigger();
                            break;
                    }
                }
            }
        }
        #endregion Properties

        #region Methods
        public void OnWindowLoaded(NewTriggerWindow myWindow)
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
