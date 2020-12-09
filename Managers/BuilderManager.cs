﻿using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class BuilderManager
    {
        public static void ManageUnits(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            IEnumerable<Entity> myBuilders = playerView.Entities.Where(e => e.IsMyEntity && e.IsBuilderUnit);

            IEnumerable<Entity> resources = playerView.Entities.Where(e => e.IsResource);

            foreach (Entity builder in myBuilders)
            {
                var resourceRanges = resources.Select(r
                    => new { r.Id, Range = r.Position.RangeTo(builder.Position) });

                if (!resourceRanges.Any())
                    break;

                double minRange = resourceRanges.Min(r => r.Range);
                int resourceId = resourceRanges.First(r => r.Range == minRange).Id;
                Entity closestResource = resources.First(r => r.Id == resourceId);

                if (builder.CanAttack(closestResource))
                {
                    var attackAction = new AttackAction { Target = closestResource.Id };

                    actions.Add(builder.Id, new EntityAction() { AttackAction = attackAction });

                    continue;
                }

                var moveAction = new MoveAction { Target = closestResource.Position };
                actions.Add(builder.Id, new EntityAction() { MoveAction = moveAction });

                continue;
            }
        }
    }
}
