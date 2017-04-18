using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.StaticData
{
    public class Champion
    {
        [JsonProperty("info")]
        public JToken Info { get; set; } // TODO

        [JsonProperty("enemytips")]
        public List<string> EnemyTips { get; set; }

        [JsonProperty("stats")]
        public JToken Stats { get; set; } // TODO

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image")]
        public JToken Image { get; set; } // TODO

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("partype")]
        public string ParType { get; set; }

        [JsonProperty("skins")]
        public JToken Skins { get; set; } // TODO

        [JsonProperty("passive")]
        public JToken Passive { get; set; } // TODO

        [JsonProperty("recommended")]
        public JToken Recommended { get; set; } // TODO

        [JsonProperty("allytips")]
        public List<string> AllyTips { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("lore")]
        public string Lore { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("blurb")]
        public string Blurb { get; set; }

        [JsonProperty("spells")]
        public List<Spell> Spells { get; set; }
    }
}
