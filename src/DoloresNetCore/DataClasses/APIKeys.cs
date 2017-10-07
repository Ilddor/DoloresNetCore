using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Dolores.DataClasses
{
    public class APIKeys : IState
    {
        public string DiscordAPIKey { get; set; }
        public string RiotAPIKey { get; set; }
        public string PUBGTrackerKey { get; set; }
        public string SteamWebAPIKey { get; set; }

        public void Save()
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

        public void Load()
        {
            try
            {
                using (Stream stream = File.Open("keys.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    var tmp = JsonConvert.DeserializeObject<APIKeys>(streamReader.ReadLine());
                    // Is here a better way to do this?
                    Type t = this.GetType();
                    PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach(var prop in properties)
                    {
                        prop.SetValue(this, prop.GetValue(tmp));
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
