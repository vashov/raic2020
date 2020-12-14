using Aicup2020;
using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class WarriorManager
    {
        public class AttackPos
        {
            public int PlayerId { get; set; }
            public bool Completed { get; set; }
            public Vec2Int Pos { get; set; }
        }

        private static readonly List<AttackPos> NeedAttackList = new List<AttackPos>
        {
            new AttackPos { PlayerId = 3, Pos = new Vec2Int(75, 5) },
            new AttackPos { PlayerId = 4, Pos = new Vec2Int(5, 75) },
            new AttackPos { PlayerId = 5, Pos = new Vec2Int(75, 75) },
        };

        public static void ManageUnits(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            if (NeedAttackList.All(p => p.Completed))
            {
                NeedAttackList.ForEach(p => p.Completed = false);
            }

            Vec2Int attackPos = NeedAttackList.First(p => !p.Completed).Pos;

            List<Entity> allWarriors = playerView.Entities.Where(e => e.IsMyEntity && e.IsWarrior).ToList();

            foreach (var pos in NeedAttackList)
            {
                if (allWarriors.Any(p => p.Position == pos.Pos))
                {
                    pos.Completed = true;
                    continue;
                }
            }

            List<Entity> freeWarriors = allWarriors.ToList();

            IEnumerable<Entity> enemiesInMyBase = playerView.Entities.Where(e => e.IsEnemyEntity && e.InMyBase());

            int warriorsCount = allWarriors.Count;
            if (warriorsCount > 20)
            {
                // SPECIAL FORCES!
                int teamSize = (warriorsCount - 5) * 60 / 100;

                var warriorsRanges = allWarriors
                    .Select(w => new { w.Id, Range = w.Position.RangeTo(attackPos) })
                    .OrderBy(r => r.Range)
                    .Take(teamSize);

                foreach (var special in warriorsRanges)
                {
                    var moveAction = new MoveAction { Target = attackPos, BreakThrough = true };
                    var attackAction = new AttackAction { AutoAttack = new AutoAttack(10, new EntityType[] { }) };

                    actions.Add(special.Id, new EntityAction()
                    {
                        MoveAction = moveAction,
                        AttackAction = attackAction
                    });

                    freeWarriors.RemoveAll(w => w.Id == special.Id);
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
