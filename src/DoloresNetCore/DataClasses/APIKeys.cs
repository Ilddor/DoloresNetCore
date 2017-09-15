using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Dolores.DataClasses
{
    public class APIKeys
    {
        public string DiscordAPIKey { get; set; }
        public string RiotAPIKey { get; set; }
        public string PUBGTrackerKey { get; set; }

        public void SaveToFile()
        {
            try
            {
                using (FileStream stream = File.Open("keys.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(this));
                    streamWriter.Flush();
                    stream.Flush();
                }
            }
            catch (Exception) { }
        }

        public static APIKeys LoadKeys()
        {
            try
            {
                using (Stream stream = File.Open("keys.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    return JsonConvert.DeserializeObject<APIKeys>(streamReader.ReadLine());
                }
            }
            catch (Exception) { }
            return null;
        }
    }
}
