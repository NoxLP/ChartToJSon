using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFHelpers;

namespace StoryChart.Model.Rewards
{
    public abstract class aNodeReward : aNotifyPropertyChanged, INodeReward
    {
        #region Fields
        private string _Description;
        private string _Id;
        private RewardsTypesEnum _Type;
        #endregion Fields

        #region Properties
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value == null)
                {
                    if (_Description != null)
                    {
                        _Description = null;
                        OnPropertyChanged(nameof(Description));
                    }
                }
                else if (!value.Equals(_Description))
                {
                    _Description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        public string Id
        {
            get { return _Id; }
            set
            {
                if (value == null)
                {
                    if (_Id != null)
                    {
                        _Id = null;
                        OnPropertyChanged(nameof(Id));
                    }
                }
                else if (!value.Equals(_Id))
                {
                    _Id = value;
                    OnPropertyChanged(nameof(Id));
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
                }
            }
        }
        #endregion Properties
    }
}
