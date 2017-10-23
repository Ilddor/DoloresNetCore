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
    public class Configuration : ModuleBase
    {
        private IServiceProvider m_Map;
        private Random m_Random = new Random();

        public Configuration(IServiceProvider map)
        {
            m_Map = map;
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
    }
}
