using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Dolores.Modules.Social;
using Dolores.EventHandlers;
using Dolores.DataClasses;

namespace Dolores.EventHandlers
{
    class CommandHandler : IInstallable
    {
        public CommandService m_Commands;
        private DiscordSocketClient m_Client;
        private IServiceProvider m_Map;

        public async Task Install(IServiceProvider map)
        {
            m_Client = map.GetService<DiscordSocketClient>();
            m_Commands = new CommandService();
            m_Map = map;

            await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly());

            m_Client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            await message.Channel.GetMessageAsync(message.Id, CacheMode.AllowDownload); // to cache it:)

            int argPos = 0;

            var context = new CommandContext(m_Client, message);
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = null;
            if (context.Channel is IDMChannel)
            {
                guildConfig = configs.GetGuildFromDMContext(m_Client, context.User.Id);

                if(guildConfig == null) // Get default values then
                    guildConfig = new Configurations.GuildConfig();
            }
            else
            {
                guildConfig = configs.GetGuildConfig(context.Guild.Id);
            }

#if _WINDOWS_
            if (!(message.HasMentionPrefix(m_Client.CurrentUser, ref argPos) || message.HasStringPrefix(guildConfig.Prefix, ref argPos)) || message.Author.IsBot) return;
#else
            if (!(message.HasMentionPrefix(m_Client.CurrentUser, ref argPos) || message.HasStringPrefix(guildConfig.Prefix, ref argPos)) || message.Author.IsBot) return;
#endif

            await message.Channel.TriggerTypingAsync();
            var result = await m_Commands.ExecuteAsync(context, argPos, m_Map);

            if (!result.IsSuccess)
            {
                if (guildConfig.CommandNotFoundEnabled)
                {
                    if (result.ErrorReason == "Unknown command.")
                        await message.Channel.SendMessageAsync($"{guildConfig.Translation.UnknownCommand}");
                    else
                        await message.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                }
            }
        }
    }
}
