﻿using Aicup2020;
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

        private static Vec2Int[] _housePlaces = new Vec2Int[26]
        {
            //new Vec2Int(12, 12),
            //new Vec2Int(11, 8),
            //new Vec2Int(8, 11),
            //new Vec2Int(11, 5),
            //new Vec2Int(5, 11),
            new Vec2Int(1, 1),
            new Vec2Int(4, 1),
            new Vec2Int(1, 4),
            new Vec2Int(1, 7),
            new Vec2Int(7, 1),
            new Vec2Int(1, 10),
            new Vec2Int(10, 1),
            new Vec2Int(1, 13),
            new Vec2Int(13, 1),
            new Vec2Int(1, 16),
            new Vec2Int(16, 1),
            new Vec2Int(19, 1),
            new Vec2Int(1, 19),

            new Vec2Int(5, 17),
            new Vec2Int(17, 5),
            new Vec2Int(17, 8),
            new Vec2Int(8, 17),

            new Vec2Int(12, 12),

            new Vec2Int(22, 1),
            new Vec2Int(1, 22),
            new Vec2Int(25, 1),
            new Vec2Int(1, 25),

            new Vec2Int(5, 20),
            new Vec2Int(20, 5),
            new Vec2Int(8, 20),
            new Vec2Int(20, 8)
        };

        private static Vec2Int[] _rangedBasePlaces = new Vec2Int[3]
        {
            new Vec2Int(11, 5),
            new Vec2Int(5, 11),
            new Vec2Int(11, 11)
        };

        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            List<Entity> allBuilders = WorldConfig.MyEntites.Where(e => e.IsBuilderUnit).ToList();

            UpdateCompletedOrders(allBuilders, playerView);
            ClearUnitOrdersFromDeadBuilders(allBuilders);

            List<Entity> enemyAttakers = playerView.Entities
                .Where(e => e.IsEnemyEntity && (e.IsTurret || e.IsWarrior))
                .ToList();

            List<int> runningFromEnemy = new List<int>();
            foreach (Entity builder in allBuilders)
            {
                var enemyDistance = enemyAttakers.Select(e => new 
                {
                    e.Position, 
                    Distance = e.Position.RangeTo(builder.Position), 
                    AttackRange = e.Properties.Attack.Value.AttackRange 
                });

                var closestEnemy = enemyDistance.FirstOrDefault(d => d.Distance - 2 < d.AttackRange);
                if (closestEnemy == null)
                {
                    continue;
                }

                int moveX = closestEnemy.Position.X > builder.Position.X ? -1
                    : closestEnemy.Position.X < builder.Position.X ? 1 : 0;

                int moveY = closestEnemy.Position.Y > builder.Position.Y ? -1
                    : closestEnemy.Position.Y < builder.Position.Y ? 1 : 0;

                int newPosX = (builder.Position.X + moveX) > WorldConfig.MapSize || (builder.Position.X + moveX) < 0 ?
                    builder.Position.X : builder.Position.X + moveX;

                int newPosY = (builder.Position.Y + moveY) > WorldConfig.MapSize || (builder.Position.Y + moveY) < 0 ?
                    builder.Position.Y : builder.Position.Y + moveY;

                var movePos = new Vec2Int(newPosX, newPosY);
                var moveAction = new MoveAction() { Target = movePos, FindClosestPosition = true };

                UnitOrder.Remove(builder.Id);

                EntityAction entityAction = new EntityAction()
                {
                    BuildAction = null,
                    MoveAction = moveAction,
                    AttackAction = null,
                    RepairAction = null
                };

                runningFromEnemy.Add(builder.Id);
                actions.Add(builder.Id, entityAction);
            }

            allBuilders.RemoveAll(e => runningFromEnemy.Contains(e.Id));
            List<Entity> freeBuilders = allBuilders.Where(b => !UnitOrder.ContainsKey(b.Id)).ToList();

            bool existsOrderOfBuild = UnitOrder.Values.Any(o => o.BuildAction != null && o.BuildAction.Value.EntityType == EntityType.RangedBase);

            int countOfWarriorBases = WorldConfig.MyEntites.Where(e => e.IsMeleeBase || e.IsRangedBase).Count();
            if (!existsOrderOfBuild && countOfWarriorBases < 1)
            {
                int costOfRangedBase = WorldConfig.EntityProperties[EntityType.RangedBase].Cost;

                bool canBuildRangedBase = WorldConfig.MyPlayer.Resource > WorldConfig.ResourcesUsedInRound + costOfRangedBase;

                if (canBuildRangedBase && SelectPositionForBuilding(playerView, EntityType.RangedBase, out Vec2Int rangedBasePos, playerView.CurrentTick))
                {
                    WorldConfig.ResourcesUsedInRound += costOfRangedBase;

                    var builderRanges = freeBuilders.Select(b => new { b.Id, Range = b.Position.RangeTo(rangedBasePos) });

                    if (builderRanges.Any())
                    {
                        double minRange = builderRanges.Min(b => b.Range);
                        int builderId = builderRanges.First(b => Math.Round(b.Range) == Math.Round(minRange)).Id;
                        Entity closestBuilder = freeBuilders.First(b => b.Id == builderId);

                        Vec2Int movePos = rangedBasePos;

                        movePos = GetMovePositionForBuildRangedBase(rangedBasePos);

                        var buildAction = new BuildAction(EntityType.RangedBase, rangedBasePos);
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

            existsOrderOfBuild = UnitOrder.Values.Any(o => o.BuildAction != null && o.BuildAction.Value.EntityType == EntityType.House);

            int costOfHouse = WorldConfig.EntityProperties[EntityType.House].Cost;
            int coustOfRangedBase = WorldConfig.EntityProperties[EntityType.RangedBase].Cost;

            bool canBuildHouseAndRangedBase = WorldConfig.MyPlayer.Resource > WorldConfig.ResourcesUsedInRound + costOfHouse + coustOfRangedBase;

            if ((canBuildHouseAndRangedBase || WorldConfig.MyPopulationUsed >= WorldConfig.MyPopulationProvided - 6) && !existsOrderOfBuild)
            {
                int countOfHouses = WorldConfig.MyEntites.Where(e => e.IsHouse).Count();

                bool canBuildHouse = WorldConfig.MyPlayer.Resource > WorldConfig.ResourcesUsedInRound + costOfHouse;

                bool needBuildHouse = countOfHouses < 6 || countOfWarriorBases > 0 || canBuildHouseAndRangedBase;

                if (canBuildHouse && needBuildHouse && SelectPositionForBuilding(playerView, EntityType.House, out Vec2Int housePosition, playerView.CurrentTick))
                {
                    WorldConfig.ResourcesUsedInRound += costOfHouse;

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
                int countOfOrdersToRepair = UnitOrder.Values
                    .Count(o => o.RepairAction != null && o.RepairAction.Value.Target == housesForRepair.Id);

                bool tooManyOrderOfRepair = countOfOrdersToRepair >= housesForRepair.AllowCountOfBuildersToRepair;

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
                    AttackAction = new AttackAction(target: null, new AutoAttack(500, new EntityType[] { EntityType.Resource, EntityType.BuilderUnit})) 
                };

                actions.Add(builder.Id, entityAction);

                continue;
            }
        }

        private static Vec2Int GetMovePositionForBuildHouse(Vec2Int housePosition)
        {
            return new Vec2Int(housePosition.X, housePosition.Y);
        }

        private static Vec2Int GetMovePositionForBuildRangedBase(Vec2Int rangedBasePosition)
        {
            return new Vec2Int(rangedBasePosition.X - 1, rangedBasePosition.Y);
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

        private static bool SelectPositionForBuilding(PlayerView playerView, EntityType buildingType, out Vec2Int position, int currentTick)
        {
            Vec2Int[] places = buildingType == EntityType.House ? _housePlaces : _rangedBasePlaces;

            int minus = 0;
            if (buildingType == EntityType.House)
            {
                if (currentTick < 300)
                    minus = 8;
                else if (currentTick < 400)
                    minus = 4;
            }

            int take = places.Length - minus;

            foreach (Vec2Int place in places.Take(take))
            {
                bool isFree = true;

                var l = playerView.Entities.Where(e => e.IsMeleeBase).ToList();
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

                    int buildingSize = playerView.EntityProperties[buildingType].Size;

                    for (int x = 0; x < buildingSize; x++)
                    {
                        for (int y = 0; y < buildingSize; y++)
                        {
                            Vec2Int placePos = place;
                            placePos.X += x;
                            placePos.Y += y;

                            if (entityPlace.Contains(placePos))
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
