using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.StaticData
{
    public class ChampionList
    {
        [JsonProperty("keys")]
        public Dictionary<string,string> Keys { get; set; }

        [JsonProperty("data")]
        public Dictionary<string,Champion> Data { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }
}
