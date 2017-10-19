using Dolores.CustomAttributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading;

namespace Dolores.DataClasses
{
    public class Configurations : IState
    {
        public class GuildConfig
        {
            public LanguageDictionary.Language Lang { get; set; }
            public string Prefix { get; set; }
            public HashSet<string> InstalledModules { get; set; }
            public dynamic Translation { get; set; }
            public bool LogsEnabled { get; set; }
            public ulong NSFWCHannelId { get; set; }
            public ulong LogChannelId { get; set; }
            public static ulong DebugChannelId { get; set; }
            public ulong PUBGTrackerChannelId { get; set; }
            public ulong CSGOTrackerChannelId { get; set; }

            public GuildConfig()
            {
                Lang = LanguageDictionary.Language.EN;
                Prefix = "!";
                InstalledModules = new HashSet<string>(StringComparer.Ordinal);
                Translation = new ExpandoObject();
                NSFWCHannelId = 272419366744883200;
                LogChannelId = 356852896559661056;
                DebugChannelId = 272513888539639818;
                CSGOTrackerChannelId = 360033939257032704;
                PUBGTrackerChannelId = 359789815576788992;
            }

            public GuildConfig ShallowCopy()
            {
                return this.MemberwiseClone() as GuildConfig;
            }
        }

        private Dictionary<ulong, GuildConfig> m_GuildConfigs = new Dictionary<ulong, GuildConfig>();
        private Mutex m_Mutex = new Mutex();

        public dynamic GetGuildConfig(ulong guild)
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

            tmp.Translation = new ExpandoObject();
            string value = null;
            foreach (var langString in Enum.GetValues(typeof(LanguageDictionary.LangString)))
            {
                value = LanguageDictionary.GetString((LanguageDictionary.LangString)Enum.Parse(typeof(LanguageDictionary.LangString), langString.ToString()), tmp.Lang);
                (tmp.Translation as ExpandoObject).TryAdd(langString.ToString(), value);
            }

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

        public static bool FindLangSummaryAttribute(Attribute x, LanguageDictionary.Language lang)
        {
            return x.GetType() == typeof(LangSummaryAttribute) && (x as LangSummaryAttribute).Lang == lang;
        }
    }
}
