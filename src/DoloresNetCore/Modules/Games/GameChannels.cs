using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Dolores.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Games
{
    [RequireInstalled]
    public class GameChannels : ModuleBase
    {
        IServiceProvider m_Map;
        public GameChannels(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("game")]
        [LangSummary(LanguageDictionary.Language.PL, "Tworzy kanał dla gry oraz przenosi wszystkich użytkowników z aktualnego kanału grających w tę samą gre co autor na nowy kanał")]
        [LangSummary(LanguageDictionary.Language.EN, "Creates voice game channel and moves there all users from your voice channel that play the same game")]
        private async Task GameStart(string mention = null)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);
            SocketUser callingUser = Context.User as SocketUser;
            if (Context.User.Username == "Ilddor" && mention != null) // change user if someone else mentioned
            {
                callingUser = await Context.Guild.GetUserAsync(ulong.Parse(mention.Replace("<@!", "").Replace("<@", "").Replace(">", ""))) as SocketUser;
            }

            if (callingUser.Game.HasValue)
            {
                string message = $"{LanguageDictionary.GetString(LanguageDictionary.LangString.Moving, guildConfig.Lang)}: {callingUser.Username}";
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
                    message += $" {LanguageDictionary.GetString(LanguageDictionary.LangString.ToChannel, guildConfig.Lang)} {callingUser.Game.Value.Name}";
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
