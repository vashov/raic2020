using Aicup2020;
using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class WarriorManager
    {
        public static void ManageUnits(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            List<Entity> allWarriors = playerView.Entities.Where(e => e.IsMyEntity && e.IsWarrior).ToList();

            List<Entity> freeWarriors = allWarriors.ToList();

            IEnumerable<Entity> enemiesInMyBase = playerView.Entities.Where(e => e.IsEnemyEntity && e.InMyBase());

            int warriorsCount = allWarriors.Count();
            if (warriorsCount > 20)
            {
                IOrderedEnumerable<Player> enemyPlayers = WorldConfig.EnemyPlayers.OrderByDescending(p => p.Score);
                foreach (Player player in enemyPlayers)
                {
                    Entity enemyBuilding = playerView.Entities
                        .Where(e => e.PlayerId == player.Id && (e.IsRangedBase || e.IsMeleeBase || e.IsBuilderBase || e.IsHouse || e.IsTurret))
                        .OrderBy(e => e.AttackPriority)
                        .FirstOrDefault();

                    if (enemyBuilding.Id <= 0)
                        continue;

                    // SPECIAL FORCES!
                    int teamSize = warriorsCount * 60 / 100;

                    var warriorsRanges = allWarriors
                        .Select(w => new { w.Id, Range = w.Position.RangeTo(enemyBuilding.Position) })
                        .OrderBy(r => r.Range)
                        .Take(teamSize);

                    foreach (var special in warriorsRanges)
                    {
                        var moveAction = new MoveAction { Target = enemyBuilding.Position, BreakThrough = true };
                        var attackAction = new AttackAction { AutoAttack = new AutoAttack(5, new EntityType[] { }) };

                        actions.Add(special.Id, new EntityAction()
                        {
                            MoveAction = moveAction,
                            AttackAction = attackAction
                        });

                        freeWarriors.RemoveAll(w => w.Id == special.Id);
                    }

                    break;
                }
            }

            foreach (Entity warrior in freeWarriors)
            {
                if (enemiesInMyBase.Any())
                {
                    var enemiesRanges = enemiesInMyBase.Select(r => new { r.Id, Range = r.Position.RangeTo(warrior.Position) });

                    double minRange = enemiesRanges.Min(r => r.Range);
                    int enemyId = enemiesRanges.First(r => r.Range == minRange).Id;
                    Entity closestEnemy = enemiesInMyBase.First(r => r.Id == enemyId);

                    var moveAction = new MoveAction { Target = closestEnemy.Position };
                    var attackAction = new AttackAction { Target = closestEnemy.Id };

                    actions.Add(warrior.Id, new EntityAction() 
                    { 
                        MoveAction = moveAction,
                        AttackAction = attackAction
                    });

                    continue;
                }
                else
                {
                    Vec2Int pointForCenter = new Vec2Int { X = 20, Y = 20 };

                    var moveAction = new MoveAction { Target = pointForCenter };
                    actions.Add(warrior.Id, new EntityAction() { MoveAction = moveAction });
                }
                
            }
        }
    }
}
