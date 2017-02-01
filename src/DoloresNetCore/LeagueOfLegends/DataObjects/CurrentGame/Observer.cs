using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dolores.LeagueOfLegends.DataObjects.CurrentGame
{
    public class Observer
    {
        [JsonProperty("encryptionKey")]
        public string EncryptionKey { get; set; }
    }
}
