using Discord.WebSocket;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.EventHandlers
{
	public class LevelHandler : IInstallable
	{
		private DiscordSocketClient m_Client;
		IServiceProvider m_Map;
		Configurations m_Configs;

		public Task Install(IServiceProvider map)
		{
			m_Map = map;
			m_Client = m_Map.GetService<DiscordSocketClient>();
			m_Client.MessageReceived += MessageReceived;
			m_Configs = m_Map.GetService<Configurations>();

			return Task.CompletedTask;
		}

		public async Task MessageReceived(SocketMessage message)
		{
			try
			{
				if (!(message.Channel is SocketGuildChannel))
					return;
				if (message.Author.IsBot)
					return;
				Configurations.GuildConfig guildConfig = m_Configs.GetGuildConfig((message.Channel as SocketGuildChannel).Guild.Id);
				ulong messageExp = (ulong)Math.Ceiling(message.Content.Length * 0.3);
				guildConfig.Levels.AddExperience(message.Author.Id, messageExp);
				m_Configs.SetGuildConfig((message.Channel as SocketGuildChannel).Guild.Id, guildConfig);
			}
			catch(Exception e)
			{

			}
		}
	}
}
