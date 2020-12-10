using Aicup2020.Model;
using System.Collections.Generic;
using System.Linq;

namespace Aicup2020
{
    public static class WorldConfig
    {
        public static bool IsInitialized { get; set; }
        public static int MyId { get; set; }
        public static Player MyPlayer { get; set; }

        public static int ResourcesUsedInRound { get; set; }

        public static IDictionary<Model.EntityType, Model.EntityProperties> EntityProperties { get; set; }
        public static List<Model.Player> EnemyPlayers { get; set; }

        public static void Init(PlayerView playerView)
        {
            ResourcesUsedInRound = 0;

            MyId = playerView.MyId;
            MyPlayer = playerView.Players.First(p => p.Id == MyId);

            if (IsInitialized)
                return;
            
            EntityProperties = playerView.EntityProperties;
            EnemyPlayers = playerView.Players.Where(p => p.Id != MyId).ToList();

            IsInitialized = true;
        }
    }
}