using Aicup2020;
using Aicup2020.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class BuilderUnitManager
    {
        private struct BuilderOrder
        {
            public BuilderOrder(BuildAction? buildAction, RepairAction? repairAction)
            {
                BuildAction = buildAction;
                RepairAction = repairAction;
            }

            public BuildAction? BuildAction { get; set; }
            public RepairAction? RepairAction { get; set; }
        }

        public static readonly List<BuildAction> BuildOrders = new List<BuildAction>();

        private static readonly Dictionary<int, BuilderOrder> UnitOrder = new Dictionary<int, BuilderOrder>();

        private static readonly List<(int, BuilderOrder)> ExecutedOrders = new List<(int, BuilderOrder)>();

        private static Vec2Int[] _housePlaces = new Vec2Int[18]
        {
            new Vec2Int(12, 12),
            new Vec2Int(12, 8),
            new Vec2Int(8, 12),
            new Vec2Int(12, 5),
            new Vec2Int(5, 12),
            new Vec2Int(0, 0),
            new Vec2Int(4, 0),
            new Vec2Int(0, 3),
            new Vec2Int(0, 6),
            new Vec2Int(7, 0),
            new Vec2Int(0, 9),
            new Vec2Int(10, 0),
            new Vec2Int(0, 12),
            new Vec2Int(13, 0),
            new Vec2Int(0, 15),
            new Vec2Int(16, 0),
            new Vec2Int(19, 0),
            new Vec2Int(18, 0)
        };

        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            List<Entity> allBuilders = WorldConfig.MyEntites.Where(e => e.IsBuilderUnit).ToList();

            UpdateCompletedOrders(allBuilders, playerView);
            ClearUnitOrdersFromDeadBuilders(allBuilders);

            List<Entity> freeBuilders = allBuilders.Where(b => !UnitOrder.ContainsKey(b.Id)).ToList();

            bool existsOrderOfBuild = UnitOrder.Values.Any(o => o.BuildAction != null && o.BuildAction.Value.EntityType == EntityType.House);

            int costOfHouse = WorldConfig.EntityProperties[EntityType.House].Cost;

            if (WorldConfig.MyPopulationUsed >= WorldConfig.MyPopulationProvided - 6 && !existsOrderOfBuild)
            {
                WorldConfig.ResourcesUsedInRound += costOfHouse;

                if (SelectPositionForBuildHouse(playerView, out Vec2Int housePosition))
                {
                    var builderRanges = freeBuilders.Select(b => new { b.Id, Range = b.Position.RangeTo(housePosition) });

                    if (builderRanges.Any())
                    {
                        double minRange = builderRanges.Min(b => b.Range);
                        int builderId = builderRanges.First(b => Math.Round(b.Range) == Math.Round(minRange)).Id;
                        Entity closestBuilder = freeBuilders.First(b => b.Id == builderId);

                        Vec2Int movePos = housePosition;

                        movePos = GetMovePositionForBuildHouse(housePosition);

                        var buildAction = new BuildAction(EntityType.House, housePosition);
                        var moveAction = new MoveAction() { Target = movePos, FindClosestPosition = false };

                        UnitOrder.Add(closestBuilder.Id, new BuilderOrder(buildAction, null));

                        EntityAction entityAction = new EntityAction()
                        {
                            BuildAction = buildAction,
                            MoveAction = moveAction
                        };

                        actions.Add(closestBuilder.Id, entityAction);

                        freeBuilders.RemoveAll(b => b.Id == closestBuilder.Id);
                    }
                }
            }

            var needRepairs = WorldConfig.MyEntites
                .Where(e => e.RepairAllowed && e.Health < e.Properties.MaxHealth);

            foreach (Entity housesForRepair in needRepairs.OrderBy(n => n.RepairPriority))
            {
                bool tooManyOrderOfRepair = UnitOrder.Values
                    .Count(o => o.RepairAction != null && o.RepairAction.Value.Target == housesForRepair.Id) >= 3;

                if (housesForRepair.Id > 0 && !tooManyOrderOfRepair)
                {
                    Vec2Int pos = housesForRepair.Position;
                    var builderRanges = freeBuilders.Select(b => new { b.Id, Range = b.Position.RangeTo(pos) });

                    if (builderRanges.Any())
                    {
                        double minRange = builderRanges.Min(b => b.Range);
                        int builderId = builderRanges.First(b => Math.Round(b.Range) == Math.Round(minRange)).Id;
                        Entity closestBuilder = freeBuilders.First(b => b.Id == builderId);

                        var repairAction = new RepairAction(target: housesForRepair.Id);
                        var moveAction = new MoveAction() { Target = pos, FindClosestPosition = true };

                        UnitOrder.Add(closestBuilder.Id, new BuilderOrder(null, repairAction));

                        EntityAction entityAction = new EntityAction()
                        {
                            RepairAction = repairAction,
                            MoveAction = moveAction
                        };

                        actions.Add(closestBuilder.Id, entityAction);

                        freeBuilders.RemoveAll(b => b.Id == closestBuilder.Id);
                    }
                }
            }

            foreach (Entity builder in freeBuilders)
            {
                EntityAction entityAction;

                entityAction = entityAction = new EntityAction() 
                { 
                    MoveAction = new MoveAction(new Vec2Int(79, 79), findClosestPosition: true, breakThrough: true),
                    AttackAction = new AttackAction(target: null, new AutoAttack(500, new EntityType[] { EntityType.Resource})) 
                };

                actions.Add(builder.Id, entityAction);

                continue;
            }
        }

        private static Vec2Int GetMovePositionForBuildHouse(Vec2Int housePosition)
        {
            if (housePosition.X == 0 && housePosition.Y == 0)
                return new Vec2Int(3, 2);

            if (housePosition.X == 0)
                return new Vec2Int(housePosition.X + 3, housePosition.Y);

            if (housePosition.Y == 0)
                return new Vec2Int(housePosition.X, housePosition.Y + 3);

            if (housePosition.X == 12)
                return new Vec2Int(housePosition.X - 1, housePosition.Y);

            if (housePosition.Y == 12)
                return new Vec2Int(housePosition.X, housePosition.Y - 1);

            return housePosition;
        }

        private static void UpdateCompletedOrders(List<Entity> allBuilders, PlayerView playerView)
        {
            List<int> idsForCleaning = new List<int>();

            foreach (KeyValuePair<int, BuilderOrder> order in UnitOrder)
            {
                if (order.Value.BuildAction != null)
                {
                    if (playerView.Entities.Any(e => e.EntityType == order.Value.BuildAction.Value.EntityType && e.IsMyEntity))
                    {
                        ExecutedOrders.Add((order.Key, order.Value));
                        idsForCleaning.Add(order.Key);
                    }
                    continue;
                }

                if (order.Value.RepairAction != null)
                {
                    if (playerView.Entities.Any(e => e.Id == order.Value.RepairAction.Value.Target && e.Health == e.Properties.MaxHealth))
                    {
                        ExecutedOrders.Add((order.Key, order.Value));
                        idsForCleaning.Add(order.Key);
                    }
                    continue;
                }
            }

            idsForCleaning.ForEach(id => UnitOrder.Remove(id));
        }

        private static bool SelectPositionForBuildHouse(PlayerView playerView, out Vec2Int position)
        {
            int houseSize = playerView.EntityProperties[EntityType.House].Size;

            foreach (Vec2Int place in _housePlaces)
            {
                bool isFree = true;

                foreach (Entity entity in playerView.Entities)
                {
                    for (int x = 0; x < houseSize; x++)
                    {
                        for (int y = 0; y < houseSize; y++)
                        {
                            var pos = entity.Position;

                            pos.X += x;
                            pos.Y += y;

                            if (pos == place)
                            {
                                isFree = false;
                                goto gotoLable;
                            }

                        }
                    }
                }

                gotoLable:

                if (isFree)
                {
                    position = place;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static void ClearUnitOrdersFromDeadBuilders(IEnumerable<Entity> allBuilders)
        {
            List<int> deadBuilders = new List<int>();
            foreach (int builderId in UnitOrder.Keys)
            {
                if (allBuilders.Any(b => b.Id == builderId))
                    continue;
                deadBuilders.Add(builderId);
            }

            deadBuilders.ForEach(id => UnitOrder.Remove(id));
        }
    }
}
