using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Rewards
{
    public class VariableReward : aNodeReward
    {
        public VariableReward()
        {
            Type = RewardsTypesEnum.Variable;
        }

        #region Fields
        private string _VariableName;
        private object _VariableValue;
        #endregion Fields

        #region Properties
        public string VariableName
        {
            get { return _VariableName; }
            set
            {
                if (value == null)
                {
                    if (_VariableName != null)
                    {
                        _VariableName = null;
                        OnPropertyChanged(nameof(VariableName));
                    }
                }
                else if (!value.Equals(_VariableName))
                {
                    _VariableName = value;
                    OnPropertyChanged(nameof(VariableName));
                }
            }
        }
        public object VariableValue
        {
            get { return _VariableValue; }
            set
            {
                if (value == null)
                {
                    if (_VariableValue != null)
                    {
                        _VariableValue = null;
                        OnPropertyChanged(nameof(VariableValue));
                    }
                }
                else if (!value.Equals(_VariableValue))
                {
                    _VariableValue = value;
                    OnPropertyChanged(nameof(VariableValue));
                }
            }
        }
        #endregion Properties
    }
}
