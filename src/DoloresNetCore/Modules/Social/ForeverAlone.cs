using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Social
{
    public class ForeverAlone
    {
        private DiscordSocketClient m_Client;
        private IServiceProvider m_Map;
        private ulong m_UserIDToFollow = 131816357980405760;

        public void Install(IServiceProvider map)
        {
            m_Client = map.GetService<DiscordSocketClient>();
            m_Map = map;
            m_Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        }

        private async Task UserVoiceStateUpdated(SocketUser unused, SocketVoiceState before, SocketVoiceState after)
        {
            SocketGuild guild = m_Client.GetGuild(269960016591716362);
            SocketUser user = guild.GetUser(m_UserIDToFollow);

            IGuildUser guildUser = user as IGuildUser;
            var usersOnVoiceChannelAsync = guildUser.VoiceChannel.GetUsersAsync();
            var usersOnVoiceChannel = await usersOnVoiceChannelAsync.Flatten();
            int usersCount = System.Linq.Enumerable.Count(usersOnVoiceChannel);
            Voice.Voice.AudioClientWrapper audioClient = m_Map.GetService<Voice.Voice.AudioClientWrapper>();
            if (usersCount == 1)
            {
                if(audioClient.m_CurrentChannel != null)
                {
                    var usersOnBotsVoiceChannelAsync = audioClient.m_CurrentChannel.GetUsersAsync();
                    var usersOnBotsVoiceChannel = await usersOnBotsVoiceChannelAsync.Flatten();
                    int usersOnBotsVoiceChannelCount = System.Linq.Enumerable.Count(usersOnBotsVoiceChannel);
                    if (usersOnBotsVoiceChannelCount > 1)
                        return;
                }
                await audioClient.JoinVoiceChannel(m_Map, guildUser.VoiceChannel);
            }
        }
    }
}
