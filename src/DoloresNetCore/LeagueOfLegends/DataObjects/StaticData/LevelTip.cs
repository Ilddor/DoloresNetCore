using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.StaticData
{
    public class LevelTip
    {
        [JsonProperty("effect")]
        public List<string> Effect { get; set; }

        [JsonProperty("label")]
        public List<string> Label { get; set; }
    }
}
