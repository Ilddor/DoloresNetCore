using System;
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
        public Task Install(IServiceProvider map)
        {
            var client = map.GetService<DiscordSocketClient>();
            client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;

            return Task.CompletedTask;
        }

        private static async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (before.VoiceChannel != after.VoiceChannel)
            {
                var createdChannels = Dolores.m_Instance.map.GetService<CreatedChannels>();
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
                    await channel.SendMessageAsync($"Usuwam kanał gry: {before.VoiceChannel.Name}");
                    await before.VoiceChannel.DeleteAsync();
                }
            }
        }
    }
}
