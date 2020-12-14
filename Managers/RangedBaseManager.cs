using Aicup2020;
using Aicup2020.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aicup2020.Managers
{
    public static class RangedBaseManager
    {
        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            IEnumerable<Entity> myRangedBases = WorldConfig.MyEntites.Where(e => e.IsRangedBase);

            int warriorsCount = WorldConfig.MyEntites.Count(e => e.IsWarrior);
            int buildersCount = WorldConfig.MyEntites.Count(e => e.IsBuilderUnit);
            int provided = WorldConfig.MyPopulationProvided;

            if (provided != 0)
            {
                decimal warriorsPercent = warriorsCount / (decimal)provided * 100;
                if (warriorsPercent >= WorldConfig.WarriorPercent)
                {
                    foreach (var rangedBase in myRangedBases)
                    {
                        actions.Add(rangedBase.Id, new EntityAction());
                    }
                    return;
                }
            }
            

            int costOfRangedUnit = WorldConfig.EntityProperties[EntityType.RangedUnit].Cost;

            foreach (Entity rangedBase in myRangedBases)
            {
                if (WorldConfig.MyPlayer.Resource - WorldConfig.ResourcesUsedInRound < costOfRangedUnit)
                    continue;

                WorldConfig.ResourcesUsedInRound += costOfRangedUnit;

                var position = GetPositionForUnit(rangedBase, playerView);

                var buildAction = new BuildAction()
                {
                    EntityType = EntityType.RangedUnit,
                    Position = position
                };

                actions.Add(rangedBase.Id, new EntityAction() { BuildAction = buildAction });

                continue;
            }
        }

        private static Vec2Int GetPositionForUnit(Entity builderBase, PlayerView playerView)
        {
            Vec2Int basePosition = builderBase.Position;

            int size = builderBase.Properties.Size;

            var newPosition = new Vec2Int(basePosition.X + size, basePosition.Y + size - 1);
            return newPosition;
        }
    }
}
