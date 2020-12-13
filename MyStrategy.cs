using aicup2020.Managers;
using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace Aicup2020
{
    public class MyStrategy
    {
        public Action GetAction(PlayerView playerView, DebugInterface debugInterface)
        {
            WorldConfig.Init(playerView);

            int entityCount = playerView.Entities.Count(e => e.IsMyEntity);

            var actions = new Dictionary<int, Model.EntityAction>(entityCount);

            var l = WorldConfig.MyEntites.ToList();
            BuilderBaseManager.Manage(playerView, actions);

            BuilderUnitManager.Manage(playerView, actions);

            RangedBaseManager.Manage(playerView, actions);

            WarriorManager.ManageUnits(playerView, actions);

            return new Action(actions);
        }

        public void DebugUpdate(PlayerView playerView, DebugInterface debugInterface) 
        {
            debugInterface.Send(new DebugCommand.Clear());
            debugInterface.GetState();
        }
    }
}