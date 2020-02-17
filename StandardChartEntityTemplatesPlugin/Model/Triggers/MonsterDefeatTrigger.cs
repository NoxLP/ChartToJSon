using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Triggers
{
    public class MonsterDefeatTrigger : NodeTriggerWithGameObject
    {
        public MonsterDefeatTrigger()
        {
            Type = TriggerTypesEnum.MonsterDefeat;
        }
    }
}
