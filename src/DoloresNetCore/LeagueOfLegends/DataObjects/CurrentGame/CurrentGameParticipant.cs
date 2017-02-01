using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.CurrentGame
{
    public class CurrentGameParticipant
    {
        [JsonProperty("profileIconId")]
        public long ProfileIconID { get; set; }

        [JsonProperty("championId")]
        public long ChampionID { get; set; }

        [JsonProperty("summonerName")]
        public string SummonerName { get; set; }

        [JsonProperty("runes")]
        public List<Rune> Runes { get; set; }

        [JsonProperty("bot")]
        public bool IsBot { get; set; }

        [JsonProperty("masteries")]
        public List<Mastery> Masteries { get; set; }

        [JsonProperty("spell1Id")]
        public long Spell1ID { get; set; }

        [JsonProperty("spell2Id")]
        public long Spell2ID { get; set; }

        [JsonProperty("teamId")]
        public long TeamID { get; set; }

        [JsonProperty("summonerId")]
        public long SummonerID { get; set; }
    }
}
