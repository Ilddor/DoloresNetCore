using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.CurrentGame
{
    public class Mastery
    {
        [JsonProperty("masteryId")]
        public long MasteryID { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
}
