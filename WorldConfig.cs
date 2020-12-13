using Aicup2020.Model;
using System;
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

        public static int MyPopulationUsed { get; set; }
        public static int MyPopulationProvided { get; set; }

        public static IEnumerable<Entity> MyEntites { get; set; }

        public static int MapSize { get; set; }

        public static decimal WarriorPercent { get; set; }

        public static void Init(PlayerView playerView)
        {
            if (!IsInitialized)
            {
                EntityProperties = playerView.EntityProperties;
                MapSize = playerView.MapSize;
                IsInitialized = true;
            }

            WarriorPercent = GetWarriorPercent(playerView.CurrentTick);

            ResourcesUsedInRound = 0;

            MyId = playerView.MyId;

            MyEntites = playerView.Entities.Where(e => e.PlayerId == MyId).ToList();

            // Get updated resources and score.
            MyPlayer = playerView.Players.First(p => p.Id == MyId);
            EnemyPlayers = playerView.Players.Where(p => p.Id != MyId).ToList();

            MyPopulationUsed = MyEntites.Sum(e => EntityProperties[e.EntityType].PopulationUse);
            MyPopulationProvided = MyEntites.Sum(e => EntityProperties[e.EntityType].PopulationProvide);
        }

        private static decimal GetWarriorPercent(int currentTick)
        {
            if (currentTick <= 50)
                return 0;

            if (currentTick <= 200)
                return 50;

            return 80;
        }
    }
}