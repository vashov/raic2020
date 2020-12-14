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

            List<Vec2Int> respawnPlaces = new List<Vec2Int>(size * 2);

            for (int x = 0; x < size; x++)
            {
                Vec2Int posX = basePosition;
                Vec2Int posY = basePosition;

                posX.X += size;
                posX.Y += (size - 1 - x);

                posY.Y += size;
                posY.X += (size - 1 - x);

                respawnPlaces.Add(posX);
                respawnPlaces.Add(posY);
            }

            foreach (Entity entity in playerView.Entities)
            {
                int entitySize = entity.Properties.Size;

                Vec2Int[] entityPlace = new Vec2Int[entitySize * entitySize];
                int index = 0;
                for (int x = 0; x < entitySize; x++)
                {
                    for (int y = 0; y < entitySize; y++)
                    {
                        Vec2Int entityPos = entity.Position;
                        entityPos.X += x;
                        entityPos.Y += y;

                        entityPlace[index++] = entityPos;
                    }
                }

                foreach(var pl in entityPlace)
                {
                    index = respawnPlaces.IndexOf(pl);
                    if (index >= 0)
                        respawnPlaces.RemoveAt(index);
                }
            }

            if (respawnPlaces.Any())
                return respawnPlaces.First();

            return new Vec2Int(basePosition.X + size, basePosition.Y - 1);
        }
    }
}
