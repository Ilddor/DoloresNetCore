using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.StaticData
{
    public class SpellVar
    {
        [JsonProperty("ranksWith")]
        public string RanksWith { get; set; }

        [JsonProperty("dyn")]
        public string Dyn { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("coeff")]
        public List<double> Coeff { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
