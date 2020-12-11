using Aicup2020;
using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class BuilderUnitManager
    {
        //private static Dictionary<int, EntityAction> _orders = new Dictionary<int, EntityAction>();

        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            IEnumerable<Entity> myBuilders = WorldConfig.MyEntites.Where(e => e.IsBuilderUnit);

            IEnumerable<Entity> resources = playerView.Entities.Where(e => e.IsResource);

            //List<int> deadBuilders = new List<int>();
            //foreach (int builderId in _orders.Keys)
            //{
            //    if (myBuilders.Any(b => b.Id == builderId))
            //        continue;
            //    deadBuilders.Add(builderId);
            //}

            //deadBuilders.ForEach(id => _orders.Remove(id));

            var resourcesUsedIds = new List<int>();

            foreach (Entity builder in myBuilders)
            {
                //if (_orders.ContainsKey(builder.Id))
                //{
                //    EntityAction order = _orders[builder.Id];
                //    if (order.AttackAction != null)
                //    {
                //        int? targetId = order.AttackAction.Value.Target;
                //        if (playerView.Entities.Any(e => e.Id == targetId))
                //        {
                //            continue;
                //        }
                //        else
                //        {
                //            _orders.Remove(builder.Id);
                //        }
                //    }
                //}

                EntityAction entityAction;

                //var houses = WorldConfig.MyEntites.Where(e => e.EntityType == EntityType.House).ToList();
                
                //bool existsOrderOfBuild = _orders
                //    .Any(o => o.Value.BuildAction != null && o.Value.BuildAction.Value.EntityType == EntityType.House);

                //if (WorldConfig.MyPopulationUsed >= WorldConfig.MyPopulationProvided - 1 && !existsOrderOfBuild)
                //{
                //    EntityProperties houseProperties = WorldConfig.EntityProperties[EntityType.House];

                //    if (WorldConfig.ResourcesUsedInRound + houseProperties.Cost <=  WorldConfig.MyPlayer.Resource)
                //    {
                //        Vec2Int position = new Vec2Int(builder.Position.X, builder.Position.Y + 1);

                //        if (!WorldConfig.MyEntites.Any(e => e.Position == position))
                //        {
                //            var buildAction = new BuildAction { EntityType = EntityType.House, Position = position };

                //            entityAction = new EntityAction() { BuildAction = buildAction };

                //            if (_orders.ContainsKey(builder.Id))
                //                _orders.Remove(builder.Id);

                //            _orders.Add(builder.Id, entityAction);
                //            actions.Add(builder.Id, entityAction);

                //            continue;
                //        }
                //    }

                //    _orders.Remove(builder.Id);
                //}

                var resourceRanges = resources.Where(r => !resourcesUsedIds.Contains(r.Id)).Select(r
                    => new { r.Id, Range = r.Position.RangeTo(builder.Position) });

                if (!resourceRanges.Any())
                    break;

                double minRange = resourceRanges.Min(r => r.Range);
                int resourceId = resourceRanges.First(r => r.Range == minRange).Id;
                Entity closestResource = resources.First(r => r.Id == resourceId);

                resourcesUsedIds.Add(resourceId);


                if (builder.CanAttack(closestResource))
                {
                    var attackAction = new AttackAction { Target = closestResource.Id };

                    entityAction = new EntityAction() { AttackAction = attackAction };

                    //_orders.Remove(builder.Id);
                    //_orders.Add(builder.Id, entityAction);

                    actions.Add(builder.Id, entityAction);

                    continue;
                }

                var moveAction = new MoveAction { Target = closestResource.Position };

                entityAction = new EntityAction() { MoveAction = moveAction };

                actions.Add(builder.Id, entityAction);

                continue;
            }
        }
    }
}
