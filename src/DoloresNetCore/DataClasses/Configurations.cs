using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Dolores.DataClasses
{
    public class Configurations : IState
    {
        public class GuildConfig
        {
            public LanguageDictionary.Language Lang = LanguageDictionary.Language.EN;

            public GuildConfig ShallowCopy()
            {
                return this.MemberwiseClone() as GuildConfig;
            }
        }

        private Dictionary<ulong, GuildConfig> m_GuildConfigs = new Dictionary<ulong, GuildConfig>();
        private Mutex m_Mutex = new Mutex();

        public GuildConfig GetGuildConfig(ulong guild)
        {
            GuildConfig tmp = null;
            m_Mutex.WaitOne();
            if (m_GuildConfigs.ContainsKey(guild))
            {
                tmp = m_GuildConfigs[guild].ShallowCopy(); // Otherwise it would be reference to Dictionary object instance?
            }
            else
            {
                tmp = new GuildConfig();
                m_GuildConfigs.Add(guild, tmp);
            }
            m_Mutex.ReleaseMutex();
            return tmp;
        }

        public void SetGuildConfig(ulong guild, GuildConfig config)
        {
            m_Mutex.WaitOne();
            if (m_GuildConfigs.ContainsKey(guild))
                m_GuildConfigs[guild] = config;
            else
                m_GuildConfigs.Add(guild, config);
            m_Mutex.ReleaseMutex();
        }

        public void Load()
        {
            m_Mutex.WaitOne();
            try
            {
                using (Stream stream = File.Open("guildConfigs.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    m_GuildConfigs = JsonConvert.DeserializeObject<Dictionary<ulong, GuildConfig>> (streamReader.ReadLine());
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public void Save()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("guildConfigs.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_GuildConfigs));
                    streamWriter.Flush();
                    stream.Flush();
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
