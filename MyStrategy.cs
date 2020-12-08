using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace Aicup2020
{
    public class MyStrategy
    {
        public Action GetAction(PlayerView playerView, DebugInterface debugInterface)
        {
            Conf.Init(playerView);

            IEnumerable<Entity> warriors = playerView.Entities.
                Where(e => !e.IsEnemy() && e.EntityType.IsWarrior());

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

    public static class EntityTypeExtension
    {
        public static bool IsWarrior(this EntityType type) => type == EntityType.MeleeUnit || type == EntityType.RangedUnit;
    }

    public static class EntityExtension
    {
        public static bool IsEnemy(this Entity entity) => entity.PlayerId != Conf.MyId && Conf.EnemyPlayers.Any(p => p.Id == entity.PlayerId);

        public static bool CanAttack(this Entity entity, Entity enemy)
        {
            EntityProperties prop = Conf.EntityProperties[entity.EntityType];

            if (prop.Attack == null)
                return false;

            return prop.Attack.Value.AttackRange >= entity.Position.RangeTo(enemy.Position);
        }
    }

    public static class Vec2IntExtension
    {
        public static double RangeTo(this Vec2Int p1, Vec2Int p2)
        {
            return System.Math.Abs(System.Math.Sqrt(System.Math.Pow(p2.X - p1.X, 2) + System.Math.Pow(p2.Y - p1.Y, 2)));
        }
    }

    public static class Conf
    {
        public static bool IsInitialized { get; set; }
        public static int MyId { get; set; }
        public static IDictionary<Model.EntityType, Model.EntityProperties> EntityProperties { get; set; }
        public static List<Model.Player> EnemyPlayers { get; set; }

        public static void Init(PlayerView playerView)
        {
            if (IsInitialized)
                return;

            MyId = playerView.MyId;
            EntityProperties = playerView.EntityProperties;
            EnemyPlayers = playerView.Players.Where(p => p.Id != MyId).ToList();

            IsInitialized = true;
        }
    }
}