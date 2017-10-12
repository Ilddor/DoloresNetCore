using Discord;
using Discord.Commands;
using Dolores.CustomAttributes;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.Modules.Misc
{
    [LangSummary(LanguageDictionary.Language.PL, "Domyślny moduł służący do zarządzania pozostałymi modułami")]
    [LangSummary(LanguageDictionary.Language.EN, "Default module to manage other modules")]
    public class ModuleInstaller : ModuleBase
    {
        private IServiceProvider m_Map;
        private CommandService m_Commands;
        private Random m_Random = new Random();

        public ModuleInstaller(CommandService commands, IServiceProvider map)
        {
            m_Map = map;
            m_Commands = commands;
        }

        [Command("installModule")]
        [LangSummary(LanguageDictionary.Language.PL, "Instaluje wybrany moduł bota dla tego serwera")]
        [LangSummary(LanguageDictionary.Language.EN, "Install given bot module for this server")]
        [RequireAdministrator]
        public async Task InstallModule(string module)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            if (m_Commands.Modules.Any(x => x.Name == module))
            {
                guildConfig.InstalledModules.Add(module);

                configs.SetGuildConfig(Context.Guild.Id, guildConfig);
            }
            else
                await Context.Channel.SendMessageAsync("There's no such module, to get list of available modules see help command");
        }

        [Command("uninstallModule")]
        [LangSummary(LanguageDictionary.Language.PL, "Odinstalowuje wybrany moduł bota dla tego serwera")]
        [LangSummary(LanguageDictionary.Language.EN, "Uninstalls given bot module for this server")]
        [RequireAdministrator]
        public async Task UninstallModule(string module)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            if (m_Commands.Modules.Any(x => x.Name == module && x.Preconditions.Any(y => y is RequireInstalledAttribute)))
            {
                guildConfig.InstalledModules.Remove(module);

                configs.SetGuildConfig(Context.Guild.Id, guildConfig);
            }
            else
                await Context.Channel.SendMessageAsync("There's no such module, to get list of available modules see help command");
        }

        [Command("showModules")]
        [LangSummary(LanguageDictionary.Language.PL, "Pokazuje dostępne moduły bota z informacją czy są zainstalowane na serwerze")]
        [LangSummary(LanguageDictionary.Language.EN, "Shows available bot modules with information if it's installed on the server")]
        public async Task ShowModules()
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);
            var embedMessage = new EmbedBuilder().WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255));
            //var message = await Context.Channel.SendMessageAsync("", embed: embedMessage);

            foreach (var module in m_Commands.Modules)
            {
                bool installed = true;
                if (module.Preconditions.Any(x => x is RequireInstalledAttribute) &&
                       !(await module.Preconditions.Where(x => x is RequireInstalledAttribute).First().CheckPermissions(Context, module.Commands.First(), m_Map)).IsSuccess)
                    installed = false;

                if (installed)
                    embedMessage.AddField($"✅{module.Name}\n", 
                        (module.Attributes.Where(x => Configurations.FindLangSummaryAttribute(x, guildConfig.Lang)).First() as LangSummaryAttribute).Summary);
                else
                    embedMessage.AddField($"❌{module.Name}\n",
                        (module.Attributes.Where(x => Configurations.FindLangSummaryAttribute(x, guildConfig.Lang)).First() as LangSummaryAttribute).Summary);
                //await message.ModifyAsync(x => x.Embed = embedMessage.Build());
            }
            await Context.Channel.SendMessageAsync("", embed: embedMessage);
        }
    }
}
