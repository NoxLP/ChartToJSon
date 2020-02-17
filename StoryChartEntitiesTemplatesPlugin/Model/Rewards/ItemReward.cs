using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Rewards
{
    public class ItemReward : NodeRewardWithGameObject
    {
        #region Constructors
        public ItemReward()
        {
            Type = RewardsTypesEnum.Item;
        }
        #endregion Constructors

        #region Fields
        private double _Quantity_Double;
        private int _Quantity_Integral;
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
        #endregion Properties
    }
}
