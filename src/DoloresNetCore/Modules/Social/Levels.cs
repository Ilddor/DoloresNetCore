using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.CustomAttributes;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.Modules.Social
{
	[RequireInstalled]
	[LangSummary(LanguageDictionary.Language.PL, "Dodaje system doświadczenia oraz poziomów wyliczanych na podstawie aktywności użytkowników")]
	[LangSummary(LanguageDictionary.Language.EN, "Adds experience and level system based on users activity")]
	[Group("Levels")]
	public class Levels : ModuleBase
    {
		private DiscordSocketClient m_Client;
		IServiceProvider m_Map;
		Configurations m_Configs;
		private Random m_Random = new Random();

		public Levels(IServiceProvider map)
		{
			m_Map = map;
			m_Client = m_Map.GetService<DiscordSocketClient>();
			m_Configs = m_Map.GetService<Configurations>();
		}

		[Command("top")]
		[Summary("")]
		[LangSummary(LanguageDictionary.Language.PL, "")]
		[LangSummary(LanguageDictionary.Language.EN, "")]
		[RequireContext(ContextType.Guild)]
		public async Task Top(int count = 10)
		{
			Configurations.GuildConfig guildConfig = m_Configs.GetGuildConfig(Context.Guild.Id);
			var embedMessage = new EmbedBuilder().WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255));

			int place = 1;
			IGuildUser user;
			foreach(var entry in guildConfig.Levels.GetTopUsers(count))
			{
				user = await Context.Guild.GetUserAsync(entry.Key);
				embedMessage.Description += $"{place.ToString()}. {user.Username} - {entry.Value} xp\n";
			}

			Context.Channel.SendMessageAsync("", embed: embedMessage.Build());
		}
	}
}
