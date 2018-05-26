using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Dolores.Modules.Misc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.EventHandlers
{
    public class PaginationHandler : IInstallable
    {
		private DiscordSocketClient m_Client;
		private IServiceProvider m_Map;
		public CommandService m_Commands;

		public Task Install(IServiceProvider map)
		{
			m_Client = map.GetService<DiscordSocketClient>();
			m_Map = map;

			m_Commands = ((CommandHandler)Dolores.m_Instance.m_Handlers.Find(x => x.GetType() == typeof(CommandHandler))).m_Commands;

			m_Client.ReactionAdded += ReactionAdded;

			return Task.CompletedTask;
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel messageChannel, SocketReaction reaction)
		{
			if (messageChannel is IGuildChannel && !reaction.User.Value.IsBot)
			{
				var configs = m_Map.GetService<Configurations>();
				Configurations.GuildConfig guildConfig = configs.GetGuildConfig((messageChannel as IGuildChannel).GuildId);

				if (guildConfig.LastHelpMessageId.HasValue &&
					guildConfig.LastHelpCommandContext != null &&
					message.Id == guildConfig.LastHelpMessageId)
				{
					message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
					int pageNum = int.Parse(message.Value.Embeds.First().Description.Split('/')[0]);

					if (reaction.Emote is Emoji &&
						(reaction.Emote as Emoji).Name == "⏭")
					{
						pageNum++;
					}
					else if (reaction.Emote is Emoji && 
						(reaction.Emote as Emoji).Name == "⏮" &&
						pageNum > 0)
					{
						pageNum--;
					}
					await message.Value.ModifyAsync(x => x.Embed = Misc.BuildHelpPage(pageNum, guildConfig, m_Commands, m_Map, guildConfig.LastHelpCommandContext));
				}
			}
		}
	}
}
