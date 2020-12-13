﻿using Aicup2020;
using Aicup2020.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class BuilderBaseManager
    {
        public static int CountOfCreatedUnit = 0;

        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            IEnumerable<Entity> myBuilderBases = WorldConfig.MyEntites.Where(e => e.IsBuilderBase);

            int countBuilders = WorldConfig.MyEntites.Where(e => e.IsBuilderUnit).Count();
            //if (countBuilders >= 5)
            //{
            //    foreach (var builderBase in myBuilderBases)
            //    {
            //        actions.Add(builderBase.Id, new EntityAction());
            //    }

            //    return;
            //}


            int costOfBuilderUnit = WorldConfig.EntityProperties[EntityType.BuilderUnit].Cost;
            foreach (Entity builderBase in myBuilderBases)
            {
                if (WorldConfig.MyPlayer.Resource - WorldConfig.ResourcesUsedInRound < costOfBuilderUnit)
                {
                    actions.Add(builderBase.Id, new EntityAction() { BuildAction = null });
                    continue;
                }

                WorldConfig.ResourcesUsedInRound += costOfBuilderUnit;

                var position = GetPositionForUnit(builderBase, playerView, countBuilders);

                var buildAction = new BuildAction()
                { 
                    EntityType = EntityType.BuilderUnit,
                    Position = position
                };

                actions.Add(builderBase.Id, new EntityAction() { BuildAction = buildAction });

                continue;
            }
        }

        private static Vec2Int GetPositionForUnit(Entity builderBase, PlayerView playerView, int countBuilders)
        {
            Vec2Int basePosition = builderBase.Position;

            int size = builderBase.Properties.Size;

            int offsetX;
            int offsetY;

            if (countBuilders % 2 == 0)
            {
                offsetX = size - 1;
                offsetY = -1;
            }
            else
            {
                offsetY = size - 1;
                offsetX = -1;
            }
             
            var newPosition = new Vec2Int(basePosition.X + offsetX, basePosition.Y + offsetY);
            return newPosition;
        }
    }
}
