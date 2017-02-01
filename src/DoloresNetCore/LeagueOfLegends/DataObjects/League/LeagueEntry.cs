using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.League
{
    public class LeagueEntry
    {
        [JsonProperty("isFreshBlood")]
        public bool IsFreshBlood { get; set; }

        [JsonProperty("isHotStreak")]
        public bool IsHotStreak { get; set; }

        [JsonProperty("isInactive")]
        public bool IsInactive { get; set; }

        [JsonProperty("isVeteran")]
        public bool IsVeteran { get; set; }

        [JsonProperty("division")]
        public string Division { get; set; }

        [JsonProperty("leaguePoints")]
        public int LeaguePoints { get; set; }

        [JsonProperty("wins")]
        public int Wins { get; set; }

        [JsonProperty("losses")]
        public int Losses { get; set; }

        [JsonProperty("playerOrTeamId")]
        public string PlayerOrTeamId { get; set; }

        [JsonProperty("playerOrTeamName")]
        public string PlayerOrTeamName { get; set; }
    }
}
