using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.CurrentGame
{
    public class BannedChampion
    {
        [JsonProperty("pickTurn")]
        public int PickTurn { get; set; }

        [JsonProperty("championId")]
        public long ChampionId { get; set; }

        [JsonProperty("teamId")]
        public long TeamID { get; set; }
    }
}
