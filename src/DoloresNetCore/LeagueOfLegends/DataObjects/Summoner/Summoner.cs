using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.Summoner
{
    public class Summoner
    {
        [JsonProperty("profileIconId")]
        public int ProfileIconID { get; set; }

        [JsonProperty("revisionDate")]
        public long RevisionDate { get; set; }

        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summonerLevel")]
        public long SummonerLevel { get; set; }
    }
}
