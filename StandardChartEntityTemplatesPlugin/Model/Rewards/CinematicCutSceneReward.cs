using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Rewards
{
    public class CinematicCutSceneReward : aNodeReward
    {
        public CinematicCutSceneReward()
        {
            Type = RewardsTypesEnum.Cinematic_CutScene;
        }

        #region Fields
        private string _CinematicCutSceneId;
        #endregion Fields

        #region Properties
        public string CinematicCutSceneId
        {
            get { return _CinematicCutSceneId; }
            set
            {
                if (value == null)
                {
                    if (_CinematicCutSceneId != null)
                    {
                        _CinematicCutSceneId = null;
                        OnPropertyChanged(nameof(CinematicCutSceneId));
                    }
                }
                else if (!value.Equals(_CinematicCutSceneId))
                {
                    _CinematicCutSceneId = value;
                    OnPropertyChanged(nameof(CinematicCutSceneId));
                }
            }
        }
        #endregion Properties
    }
}
