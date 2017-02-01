using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.League
{
    public class League
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("queue")]
        public string Queue { get; set; }

        [JsonProperty("entries")]
        public List<LeagueEntry> Entries { get; set; }
    }
}
