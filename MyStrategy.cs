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

            int entityCount = playerView.Entities.Count(e => e.IsMyEntity);

            var actions = new Dictionary<int, Model.EntityAction>(entityCount);

            ManageBuilderUnits(playerView, actions);

            IEnumerable<Entity> warriors = playerView.Entities.
                Where(e => !e.IsEnemyEntity && e.IsWarrior);

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

        private void ManageBuilderUnits(PlayerView playerView, Dictionary<int, EntityAction> actions)
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

        public void DebugUpdate(PlayerView playerView, DebugInterface debugInterface) 
        {
            debugInterface.Send(new DebugCommand.Clear());
            debugInterface.GetState();
        }
    }
}