using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Rewards
{
    public class SkillReward : aNodeReward
    {
        #region Constructors
        public SkillReward()
        {
            Type = RewardsTypesEnum.Skill;
        }
        #endregion Constructors

        #region Fields
        private double _Quantity_Double;
        private int _Quantity_Integral;
        private string _SkillId;
        #endregion Fields

        #region Properties
        public double Quantity_Double
        {
            get { return _Quantity_Double; }
            set
            {
                if (value != _Quantity_Double)
                {
                    _Quantity_Double = value;
                    OnPropertyChanged(nameof(Quantity_Double));
                    Quantity_Integral = 0;
                }
            }
        }
        public int Quantity_Integral
        {
            get { return _Quantity_Integral; }
            set
            {
                if (value != _Quantity_Integral)
                {
                    _Quantity_Integral = value;
                    OnPropertyChanged(nameof(Quantity_Integral));
                    Quantity_Double = 0;
                }
            }
        }
        public string SkillId
        {
            get { return _SkillId; }
            set
            {
                if (value == null)
                {
                    if (_SkillId != null)
                    {
                        _SkillId = null;
                        OnPropertyChanged(nameof(SkillId));
                    }
                }
                else if (!value.Equals(_SkillId))
                {
                    _SkillId = value;
                    OnPropertyChanged(nameof(SkillId));
                }
            }
        }
        #endregion Properties
    }
}
