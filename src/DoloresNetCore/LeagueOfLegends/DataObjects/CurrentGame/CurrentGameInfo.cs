using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.CurrentGame
{
    public class CurrentGameInfo
    {
        [JsonProperty("gameId")]
        public long GameID { get; set; }

        [JsonProperty("gameStartTime")]
        public long GameStartTime { get; set; }

        [JsonProperty("platformId")]
        public string PlatformID { get; set; }

        [JsonProperty("gameMode")]
        public string GameMode { get; set; }

        [JsonProperty("mapId")]
        public long MapID { get; set; }

        [JsonProperty("gameType")]
        public string GameType { get; set; }

        [JsonProperty("bannedChampions")]
        public List<BannedChampion> BannedChampions { get; set; }

        [JsonProperty("observers")]
        public Observer Observers { get; set; }

        [JsonProperty("participants")]
        public List<CurrentGameParticipant> Participants { get; set; }

        [JsonProperty("gameLength")]
        public long GameLength { get; set; }

        [JsonProperty("gameQueueConfigId")]
        public long GameQueueConfigID { get; set; }
    }
}
