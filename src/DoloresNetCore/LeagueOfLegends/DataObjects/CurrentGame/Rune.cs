using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.CurrentGame
{
    public class Rune
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("runeId")]
        public long RuneID { get; set; }
    }
}
