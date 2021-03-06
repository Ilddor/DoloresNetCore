﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dolores.DataClasses;

namespace Dolores.EventHandlers
{
    class GameChannelsHandler : IInstallable
    {
        private IServiceProvider m_Map;

        public Task Install(IServiceProvider map)
        {
            m_Map = map;
            var client = m_Map.GetService<DiscordSocketClient>();
            client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;

            return Task.CompletedTask;
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig((user as IGuildUser).Guild.Id);
            if (before.VoiceChannel != after.VoiceChannel)
            {
                var createdChannels = m_Map.GetService<CreatedChannels>();
                bool delete = false;
                createdChannels.m_Mutex.WaitOne();
                try
                {
                    if (createdChannels.m_Channels.ContainsKey(before.VoiceChannel.Id) && before.VoiceChannel.Users.Count == 0)
                    {
                        createdChannels.m_Channels.Remove(before.VoiceChannel.Id);
                        delete = true;
                    }
                }
                catch (Exception) { }
                createdChannels.m_Mutex.ReleaseMutex();
                if (delete)
                {
                    ITextChannel channel = await (user as IGuildUser).Guild.GetDefaultChannelAsync();
                    await channel.SendMessageAsync($"{guildConfig.Translation.RemovingChannel}: {before.VoiceChannel.Name}");
                    await before.VoiceChannel.DeleteAsync();
                }
            }
        }
    }
}
