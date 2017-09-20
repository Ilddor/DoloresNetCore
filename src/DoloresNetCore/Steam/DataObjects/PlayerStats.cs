using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dolores.Steam.DataObjects
{
    public class PlayerStats
    {
        [JsonProperty("steamID")]
        public string SteamID { get; set; }

        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("stats")]
        public List<GameStat> Stats { get; set; }

        [JsonProperty("achievements")]
        public List<Achievement> Achievements { get; set; }

        public class GameStat
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
        }

        public class Achievement
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("achieved")]
            public bool Achieved { get; set; }
        }
    }
}
