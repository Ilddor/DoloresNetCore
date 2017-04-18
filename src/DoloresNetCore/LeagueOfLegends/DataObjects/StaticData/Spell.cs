using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.StaticData
{
    public class Spell
    {
        [JsonProperty("cooldownBurn")]
        public string CooldownBurn { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("leveltip")]
        public JToken Leveltip { get; set; } // TODO

        [JsonProperty("vars")]
        public List<SpellVar> Vars { get; set; }

        [JsonProperty("costType")]
        public string CostType { get; set; }

        [JsonProperty("image")]
        public JToken Image { get; set; } // TODO

        [JsonProperty("sanitizedDescription")]
        public string SanitizedDescription { get; set; }

        [JsonProperty("sanitizedTooltip")]
        public string SanitizedTooltip { get; set; }

        [JsonProperty("effect")]
        public List<List<double>> Effect { get; set; }

        [JsonProperty("tooltip")]
        public string Tooltip { get; set; }

        [JsonProperty("maxrank")]
        public int MaxRank { get; set; }

        [JsonProperty("costBurn")]
        public string CostBurn { get; set; }

        [JsonProperty("rangeBurn")]
        public string RangeBurn { get; set; }

        [JsonProperty("range")]
        public object Range { get; set; }

        [JsonProperty("cooldown")]
        public List<double> Cooldown { get; set; }

        [JsonProperty("cost")]
        public List<int> Cost { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("effectBurn")]
        public List<string> EffectBurn { get; set; }

        [JsonProperty("altimages")]
        public List<JToken> AltImages { get; set; } // TODO

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
