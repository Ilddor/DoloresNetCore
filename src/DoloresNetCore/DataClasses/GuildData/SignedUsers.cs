using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class SignedUsers
    {
        [JsonProperty("UserIDs")]
        public Dictionary<ulong, bool> m_Users = new Dictionary<ulong, bool>();
        public Mutex m_Mutex = new Mutex();
    }
}
