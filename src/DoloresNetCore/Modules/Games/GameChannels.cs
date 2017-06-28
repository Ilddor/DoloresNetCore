using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Games
{
    public class GameChannels : ModuleBase
    {
        IServiceProvider m_Map;
        public GameChannels(IServiceProvider map)
        {
            m_Map = map;
        }

        static public void Install(IServiceProvider map)
        {
            var client = map.GetService<DiscordSocketClient>();
            client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        private static async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if(before.VoiceChannel != after.VoiceChannel)
            {
                var createdChannels = Dolores.m_Instance.map.GetService<CreatedChannels>();
                bool delete = false;
                createdChannels.m_Mutex.WaitOne();
                try
                {
                    if (createdChannels.m_Channels.ContainsKey(before.VoiceChannel.Id) && !before.VoiceChannel.Users.Any())
                    {
                        createdChannels.m_Channels.Remove(before.VoiceChannel.Id);
                        delete = true;
                    }
                }
                catch (Exception) { }
                createdChannels.m_Mutex.ReleaseMutex();
                if(delete)
                {
                    ITextChannel channel = await (user as IGuildUser).Guild.GetDefaultChannelAsync();
                    await channel.SendMessageAsync($"Usuwam kanał gry: {before.VoiceChannel.Name}");
                    await before.VoiceChannel.DeleteAsync();
                }
            }
        }

        [Command("game")]
        [Summary("Tworzy kanał dla gry oraz przenosi wszystkich użytkowników z aktualnego kanału grających w tę samą gre co autor na nowy kanał")]
        private async Task GameStart(string mention = null)
        {
            SocketUser callingUser = Context.User as SocketUser;
            if (Context.User.Username == "Ilddor" && mention != null) // change user if someone else mentioned
            {
                callingUser = await Context.Guild.GetUserAsync(ulong.Parse(mention.Replace("<@!", "").Replace("<@", "").Replace(">", ""))) as SocketUser;
            }

            if (callingUser.Game.HasValue)
            {
                string message = $"Przenoszę: {callingUser.Username}";
                bool success = true;
                IVoiceChannel newChannel = await Context.Guild.CreateVoiceChannelAsync(callingUser.Game.Value.Name);
                try
                {
                    List<Task> moves = new List<Task>();
                    foreach (SocketUser user in ((callingUser as IGuildUser).VoiceChannel as SocketChannel).Users)
                    {
                        if (user != callingUser &&
                            user.Game.HasValue &&
                            user.Game.Value.Name == callingUser.Game.Value.Name)
                        {
                            message += $", {user.Username}";
                            await (user as IGuildUser).ModifyAsync((e) => { e.Channel = new Optional<IVoiceChannel>(newChannel); });
                        }
                    }
                    message += $" na kanał gry {callingUser.Game.Value.Name}";
                    foreach (var it in moves)
                    {
                        it.Wait();
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    success = false;
                }

                if (success)
                {
                    var createdChannels = m_Map.GetService<CreatedChannels>();
                    createdChannels.m_Mutex.WaitOne();
                    try
                    {
                        createdChannels.m_Channels.Add(newChannel.Id, true);
                    }
                    catch (Exception) { }
                    createdChannels.m_Mutex.ReleaseMutex();
                    try
                    {
                        await (callingUser as IGuildUser).ModifyAsync((e) => { e.Channel = new Optional<IVoiceChannel>(newChannel); });
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                        createdChannels.m_Mutex.WaitOne();
                        try
                        {
                            createdChannels.m_Channels.Remove(newChannel.Id);
                        }
                        catch (Exception) { }
                        createdChannels.m_Mutex.ReleaseMutex();
                        await newChannel.DeleteAsync();
                    }
                }
                await Context.Channel.SendMessageAsync(message);
            }
        }
    }
}
