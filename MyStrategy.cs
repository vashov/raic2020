using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace Aicup2020
{
    public class MyStrategy
    {
        public Action GetAction(PlayerView playerView, DebugInterface debugInterface)
        {
            WorldConfig.Init(playerView);

            IEnumerable<Entity> warriors = playerView.Entities.
                Where(e => !e.IsEnemy() && e.IsWarrior());

            IOrderedEnumerable<Entity> enemies = playerView.Entities
                .Where(e => e.IsEnemy()).OrderBy(e => e.PlayerId).ThenBy(e => e.Id);

            //IOrderedEnumerable<Entity> enemyBuildings = playerView.Entities
            //    .Where(e => e.IsEnemy()).OrderBy(e => e.PlayerId).ThenBy(e => e.Id);

            var actions = new Dictionary<int, Model.EntityAction>(warriors.Count());

            foreach (Entity warrior in warriors)
            {
                if (!enemies.Any())
                    break;

                var enemy = enemies.First();

                if (warrior.CanAttack(enemy))
                {
                    var action = new EntityAction()
                    {
                        AttackAction = new AttackAction
                        {
                            Target = enemy.Id
                        }
                    };

                    actions.Add(warrior.Id, action);
                    continue;
                }

                {
                    var action = new EntityAction()
                    {
                        MoveAction = new MoveAction
                        {
                            Target = enemy.Position
                        }
                    };

                    actions.Add(warrior.Id, action);
                }
            }

            return new Action(actions);
        }

        public void DebugUpdate(PlayerView playerView, DebugInterface debugInterface) 
        {
            debugInterface.Send(new DebugCommand.Clear());
            debugInterface.GetState();
        }
    }
}