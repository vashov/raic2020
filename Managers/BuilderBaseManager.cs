using Aicup2020;
using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class BuilderBaseManager
    {
        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            IEnumerable<Entity> myBuilderBases = WorldConfig.MyEntites.Where(e => e.IsBuilderBase);

            int countBuilders = WorldConfig.MyEntites.Where(e => e.IsBuilderUnit).Count();
            if (countBuilders >= 5)
            {
                foreach (var builderBase in myBuilderBases)
                {
                    actions.Add(builderBase.Id, new EntityAction());
                }

                return;
            }


            int costOfBuilderUnit = WorldConfig.EntityProperties[EntityType.BuilderUnit].Cost;

            foreach (Entity builderBase in myBuilderBases)
            {
                if (WorldConfig.MyPlayer.Resource - WorldConfig.ResourcesUsedInRound < costOfBuilderUnit)
                    continue;

                WorldConfig.ResourcesUsedInRound += costOfBuilderUnit;

                var position = GetPositionForUnit(builderBase, playerView);

                var buildAction = new BuildAction()
                { 
                    EntityType = EntityType.BuilderUnit,
                    Position = position
                };

                actions.Add(builderBase.Id, new EntityAction() { BuildAction = buildAction });

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
