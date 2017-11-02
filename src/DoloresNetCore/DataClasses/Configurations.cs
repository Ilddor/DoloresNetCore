using Discord.WebSocket;
using Dolores.CustomAttributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
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
            public HashSet<string> InstalledModules { get; }

            public BannedSubreddits BannedSubreddits { get; }
            public Reactions Reactions { get; }
            public SignedUsers SignedUsers { get; }

            [JsonIgnore]
            public dynamic Translation { get; }

            public ulong? GiveawayEntitledRole { get; set; }

            public ulong? NSFWCHannelId { get; set; }
            public ulong? LogChannelId { get; set; }
            public ulong? PUBGTrackerChannelId { get; set; }
            public ulong? CSGOTrackerChannelId { get; set; }

            public static ulong DebugChannelId = 357908791745839104;

            public GuildConfig()
            {
                Lang = LanguageDictionary.Language.EN;
                Prefix = "!";
                InstalledModules = new HashSet<string>(StringComparer.Ordinal);

                BannedSubreddits = new BannedSubreddits();
                Reactions = new Reactions();
                SignedUsers = new SignedUsers();

                Translation = new ExpandoObject();

                GiveawayEntitledRole = null;

                NSFWCHannelId = null;
                LogChannelId = null;
                CSGOTrackerChannelId = null;
                PUBGTrackerChannelId = null;
            }

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

            string value = null;
            foreach (var langString in Enum.GetValues(typeof(LanguageDictionary.LangString)))
            {
                value = LanguageDictionary.GetString((LanguageDictionary.LangString)Enum.Parse(typeof(LanguageDictionary.LangString), langString.ToString()), tmp.Lang);
                if (!(tmp.Translation as ExpandoObject).TryAdd(langString.ToString(), value))
                    (tmp.Translation as IDictionary<string, Object>)[langString.ToString()] = value;
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
                    m_GuildConfigs = JsonConvert.DeserializeObject<Dictionary<ulong, GuildConfig>> (streamReader.ReadToEnd());
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
                    streamWriter.Write(JsonConvert.SerializeObject(m_GuildConfigs, Formatting.Indented));
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

        public GuildConfig GetGuildFromDMContext(DiscordSocketClient client, ulong userId)
        {
            // So far search in guilds voice channels for one that user is in
            foreach (var guild in client.Guilds)
            {
                if (guild.Users.Where(x => x.Id == userId && x.VoiceChannel != null).Any())
                {
                    return GetGuildConfig(guild.Id);
                }
            }
            return null;
        }
    }
}
