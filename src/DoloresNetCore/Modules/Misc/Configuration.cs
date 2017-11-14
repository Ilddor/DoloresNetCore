using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.CustomAttributes;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.Modules.Misc
{
    [LangSummary(LanguageDictionary.Language.PL, "Domyślny moduł służący do zarządzania konfiguracją bota dla serwera")]
    [LangSummary(LanguageDictionary.Language.EN, "Default module to manage bot configuration for this server")]
    [Group("Config")]
    public class Configuration : ModuleBase
    {
        private IServiceProvider m_Map;
        private Random m_Random = new Random();

        public Configuration(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("show")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozkazuje aktualną konfigurację dla tego serwera")]
        [LangSummary(LanguageDictionary.Language.EN, "Shows current configuration for this server")]
        [RequireAdministrator]
        public async Task Show()
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            string message = "";
            foreach(var property in guildConfig.GetType().GetProperties())
            {
                if (property.SetMethod == null)
                    continue;

                if (property.GetValue(guildConfig) != null)
                    message += $"{property.Name} = {property.GetValue(guildConfig).ToString()}\n";
                else
                    message += $"{property.Name} = Not set\n";
            }

            await Context.Channel.SendMessageAsync(message);
        }

        [Command("setLang")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić język jakim będzie posługiwać się bot na tym serwerze")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set language in which bot will operate on this server")]
        [RequireAdministrator]
        public async Task SetLang(LanguageDictionary.Language lang)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            guildConfig.Lang = lang;

            configs.SetGuildConfig(Context.Guild.Id, guildConfig);
        }

        [Command("setPrefix")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić jakiego prefixu będzie nasłuchiwać bot")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set bot prefix to use commands")]
        [RequireAdministrator]
        public async Task SetPrefix(string prefix)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            guildConfig.Prefix = prefix;

            configs.SetGuildConfig(Context.Guild.Id, guildConfig);
        }

        [Command("setNSFWChannel")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić jaki kanał będzie używany przez bota jako NSFW")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set by bot as a NSFW channel")]
        [RequireAdministrator]
        public async Task SetNSFW(string mentionString)
        {
            if(Context.Message.MentionedChannelIds.Count > 0)
            {
                var configs = m_Map.GetService<Configurations>();
                Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

                var enumerator = Context.Message.MentionedChannelIds.GetEnumerator();
                enumerator.MoveNext();
                guildConfig.NSFWCHannelId = enumerator.Current;

                configs.SetGuildConfig(Context.Guild.Id, guildConfig);
            }
        }

        [Command("setPUBGChannel")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić jaki kanał będzie używany przez bota do zmieszczania statystyk PUBG")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set by bot to put PUBG statistics")]
        [RequireAdministrator]
        public async Task SetPUBG(string mentionString)
        {
            if (Context.Message.MentionedChannelIds.Count > 0)
            {
                var configs = m_Map.GetService<Configurations>();
                Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

                var enumerator = Context.Message.MentionedChannelIds.GetEnumerator();
                enumerator.MoveNext();
                guildConfig.PUBGTrackerChannelId = enumerator.Current;

                configs.SetGuildConfig(Context.Guild.Id, guildConfig);
            }
        }

        [Command("setCSGOChannel")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić jaki kanał będzie używany przez bota do zmieszczania statystyk CSGO")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set by bot to put CSGO statistics")]
        [RequireAdministrator]
        public async Task SetCSGO(string mentionString)
        {
            if (Context.Message.MentionedChannelIds.Count > 0)
            {
                var configs = m_Map.GetService<Configurations>();
                Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

                var enumerator = Context.Message.MentionedChannelIds.GetEnumerator();
                enumerator.MoveNext();
                guildConfig.CSGOTrackerChannelId = enumerator.Current;

                configs.SetGuildConfig(Context.Guild.Id, guildConfig);
            }
        }

        [Command("commandNotFound")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić czy bot powinien reagować na wezwanie jeśli nie znaleziono komendy")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set if bot should respond to call if command was not found")]
        [RequireAdministrator]
        public async Task SetCommandNotFound(bool enabled)
        {
            if (Context.Message.MentionedChannelIds.Count > 0)
            {
                var configs = m_Map.GetService<Configurations>();
                Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

                guildConfig.CommandNotFoundEnabled = enabled;

                configs.SetGuildConfig(Context.Guild.Id, guildConfig);
            }
        }
    }
}
