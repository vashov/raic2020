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

            IOrderedEnumerable<Entity> enemies = playerView.Entities
                .Where(e => e.IsEnemyEntity).OrderBy(e => e.PlayerId).ThenBy(e => e.Id);

            //IOrderedEnumerable<Entity> enemyBuildings = playerView.Entities
            //    .Where(e => e.IsEnemy()).OrderBy(e => e.PlayerId).ThenBy(e => e.Id);

            foreach (Entity warrior in warriors)
            {
                if (!enemies.Any())
                    break;

                var enemy = enemies.First();

                if (warrior.CanAttack(enemy))
                {
                    var attackAction = new AttackAction { Target = enemy.Id };

                    actions.Add(warrior.Id, new EntityAction() { AttackAction = attackAction });

                    continue;
                }

                var moveAction = new MoveAction { Target = enemy.Position };
                actions.Add(warrior.Id, new EntityAction() { MoveAction = moveAction });
            }
        }
    }
}
