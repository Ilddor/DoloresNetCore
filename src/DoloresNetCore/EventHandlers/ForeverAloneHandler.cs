using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Dolores.EventHandlers;

namespace Dolores.Modules.Social
{
    public class ForeverAloneHandler : IInstallable
    {
        private DiscordSocketClient m_Client;
        private IServiceProvider m_Map;
        private ulong m_UserIDToFollow = 131816357980405760; // This is only for me so I don't think I need to move it to config

        public Task Install(IServiceProvider map)
        {
            m_Client = map.GetService<DiscordSocketClient>();
            m_Map = map;
            m_Client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            return Task.CompletedTask;
        }

        private async Task UserVoiceStateUpdated(SocketUser updatedUser, SocketVoiceState before, SocketVoiceState after)
        {
            SocketGuild guild = m_Client.GetGuild(269960016591716362);
            SocketUser user = guild.GetUser(m_UserIDToFollow);

            IGuildUser guildUser = user as IGuildUser;
            if (guildUser.VoiceChannel != null)
            {
                var usersOnVoiceChannelAsync = guildUser.VoiceChannel.GetUsersAsync();
                var usersOnVoiceChannel = await usersOnVoiceChannelAsync.Flatten();
                int usersCount = System.Linq.Enumerable.Count(usersOnVoiceChannel);
                Voice.Voice.AudioClientWrapper audioClient = m_Map.GetService<Voice.Voice.AudioClientWrapper>();
                if (usersCount == 1)
                {
                    bool follow = true;
                    if(audioClient.m_CurrentChannel != null && audioClient.m_CurrentChannel.Id == guildUser.VoiceChannel.Id)
                    {
                        follow = false;
                    }
                    if (audioClient.m_CurrentChannel != null && audioClient.m_CurrentChannel.Id != guildUser.VoiceChannel.Id)
                    {
                        var usersOnBotsVoiceChannelAsync = audioClient.m_CurrentChannel.GetUsersAsync();
                        var usersOnBotsVoiceChannel = await usersOnBotsVoiceChannelAsync.Flatten();
                        int usersOnBotsVoiceChannelCount = System.Linq.Enumerable.Count(usersOnBotsVoiceChannel);
                        if (usersOnBotsVoiceChannelCount > 1)
                            follow = false;
                    }
                    if (follow)
                    {
                        if (audioClient.m_Playing)
                        {
                            audioClient.StopPlay(m_Map);
                        }
                        audioClient.JoinVoiceChannel(m_Map, guildUser.VoiceChannel);
                    }
                }
            }
            return;
        }
    }
}
