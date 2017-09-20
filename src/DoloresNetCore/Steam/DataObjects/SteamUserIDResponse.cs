using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dolores.Steam.DataObjects
{
    public class SteamUserIDResponse
    {
        [JsonProperty("steamid")]
        public string SteamID { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
