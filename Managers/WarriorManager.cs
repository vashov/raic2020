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
            IEnumerable<Entity> warriors = playerView.Entities.Where(e => e.IsMyEntity && e.IsWarrior);

            IEnumerable<Entity> enemiesInMyBase = playerView.Entities.Where(e => e.IsEnemyEntity && e.InMyBase());

            //IOrderedEnumerable<Entity> enemyBuildings = playerView.Entities
            //    .Where(e => e.IsEnemy()).OrderBy(e => e.PlayerId).ThenBy(e => e.Id);

            foreach (Entity warrior in warriors)
            {
                if (enemiesInMyBase.Any())
                {
                    var enemiesRanges = enemiesInMyBase.Select(r => new { r.Id, Range = r.Position.RangeTo(warrior.Position) });

                    double minRange = enemiesRanges.Min(r => r.Range);
                    int enemyId = enemiesRanges.First(r => r.Range == minRange).Id;
                    Entity closestEnemy = enemiesInMyBase.First(r => r.Id == enemyId);

                    if (warrior.CanAttack(closestEnemy))
                    {
                        var attackAction = new AttackAction { Target = closestEnemy.Id };

                        actions.Add(warrior.Id, new EntityAction() { AttackAction = attackAction });

                        continue;
                    }

                    var moveAction = new MoveAction { Target = closestEnemy.Position };
                    actions.Add(warrior.Id, new EntityAction() { MoveAction = moveAction });

                    continue;
                }
                else
                {
                    //int halfMap = WorldConfig.MapSize / 2;

                    Vec2Int pointForCenter = new Vec2Int { X = 20, Y = 20 };

                    var moveAction = new MoveAction { Target = pointForCenter };
                    actions.Add(warrior.Id, new EntityAction() { MoveAction = moveAction });
                }
                
            }
        }
    }
}
