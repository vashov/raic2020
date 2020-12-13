using Aicup2020;
using Aicup2020.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace aicup2020.Managers
{
    public static class TurretManager
    {
        public static void Manage(PlayerView playerView, Dictionary<int, EntityAction> actions)
        {
            IEnumerable<Entity> turrets = WorldConfig.MyEntites.Where(e => e.IsTurret);

            foreach (Entity turret in turrets)
            {
                var entityAction = new EntityAction
                {
                    AttackAction = new AttackAction
                    {
                        AutoAttack = new AutoAttack(100, Array.Empty<EntityType>())
                    }
                };

                actions.Add(turret.Id, entityAction);
            }
        }
    }
}
