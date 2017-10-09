﻿using System;
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

namespace Dolores
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

            int argPos = 0;

#if _WINDOWS_
            if (!(message.HasMentionPrefix(m_Client.CurrentUser, ref argPos) || message.HasCharPrefix('?', ref argPos))) return;
#else
            if (!(message.HasMentionPrefix(m_Client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos))) return;
#endif

            var typingState = message.Channel.EnterTypingState();
            var context = new CommandContext(m_Client, message);
            var result = await m_Commands.ExecuteAsync(context, argPos, m_Map);

            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(context.Guild.Id);

            if (!result.IsSuccess)
            {
                if(result.ErrorReason == "Unknown command.")
                    await message.Channel.SendMessageAsync($"{LanguageDictionary.GetString(LanguageDictionary.LangString.UnknownCommand, guildConfig.Lang)}");
                else
                    await message.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
            typingState.Dispose();
        }
    }
}
