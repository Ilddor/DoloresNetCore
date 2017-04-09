using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.CustomAttributes;

namespace Dolores.Modules.Misc
{
    public class Misc : ModuleBase
    {
        IDependencyMap m_Map;
        CommandService m_Commands;
        private Random m_Random = new Random();
        private Dictionary<string, bool> m_blacklistedUsers;
        private Mutex m_usersMutex;

        public Misc(CommandService commands, IDependencyMap map)
        {
            m_Map = map;
            m_Commands = commands;
        }

        [Command("help")]
        [Alias("analiza")]
        [Summary("Wyświetla ten tekst")]
        public async Task Help()
        {
            string message = "Dostępne komendy:\n";
            foreach(var it in m_Commands.Commands)
            {
                if(!it.Preconditions.Any(x => x is HiddenAttribute))
                    message += $" -`!{it.Name}`    - {it.Summary}\n";
            }
            message += $"\n\n";
            var uptime = DateTime.Now - Dolores.m_Instance.m_StartTime;
            message += $"Czas online(bota): {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m\n";
            message += $"Wersja: {Dolores.m_Instance.m_Version}\n";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("ping")]
        [Summary("Umożliwia sprawdzenie działania bota")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("pong");
        }

        [Command("rawr")]
        [Summary("Zdjecie Dolores")]
        [Hidden]
        public async Task Rawr()
        {
            await Context.Channel.SendFileAsync($"Dolores{m_Random.Next(1,7)}.jpg");
        }

        [Command("goodnight")]
        [Summary("Zdjecie Dolores")]
        [Hidden]
        public async Task Goodnight()
        {
            await Context.Channel.SendFileAsync($"DoloresGoodnight{m_Random.Next(1, 3)}.jpg");
        }

        [Command("bleh")]
        [Hidden]
        public async Task bleh()
        {
            var client = m_Map.Get<DiscordSocketClient>();
            var channel = client.GetChannel(273786708120829952);
            var text = channel as ITextChannel;
            var message = await text.GetMessageAsync(299580095868305409);
            await message.DeleteAsync();
            await Context.Message.DeleteAsync();
        }

        [Command("roll")]
        [Summary("Losuje liczbę z podanego przedziału (100 jeśli nie zdefiniowano)")]
        private async Task Roll(int max = 100)
        {
            await Context.Channel.SendMessageAsync($"{Context.User.Username}: {m_Random.Next(0, max)}");
        }

        [Command("rollChannel")]
        [Summary("Losuje liczby dla całego kanału głosowego oraz wyświetla je w kolejności malejącej")]
        private async Task RollChannel()
        {
            SortedDictionary<int, string> results = new SortedDictionary<int, string>();
            int roll = 0;
            foreach (SocketGuildUser user in (Context.User as SocketGuildUser).VoiceChannel.Users)
            {
                do
                {
                    roll = m_Random.Next(0, 100);
                }
                while (results.ContainsKey(roll));

                results.Add(roll, user.Mention);
            }
            string message = "Wyniki:\n";
            foreach (var it in results.OrderByDescending(x => x.Key))
            {
                message += $"{it.Value}: {it.Key}\n";
            }
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("removeHistory")]
        [Summary("Usuwa historie n wiadomości")]
        [RequireOwner]
        private async Task RemoveHistory(int count, bool allUsers = false, string channelName = null)
        {
            SocketTextChannel channel;
            if (channelName == null)
                channel = Context.Channel as SocketTextChannel;
            else
            {
                var textChannels = await Context.Guild.GetTextChannelsAsync();
                channel = (from chan in textChannels where chan.Mention == channelName select chan) as SocketTextChannel;
            }
            var messages = channel.GetMessagesAsync(count);
            await messages.ForEachAsync(async x =>
            {
                foreach (var it in x)
                {
                    if ((it.Author.Id == m_Map.Get<DiscordSocketClient>().CurrentUser.Id) || allUsers)
                    {
                        await it.DeleteAsync();
                    }
                }
            });
        }

        [Command("abl")]
        [Summary("Dodaje osobę na czarną listę")]
        [RequireOwner]
        private async Task AddBlackList(string name)
        {
            m_blacklistedUsers.Add(name, true);
            await Context.Channel.SendMessageAsync($"Ok");
        }

        [Command("rbl")]
        [Summary("Usuwa osobę z czarnej listy")]
        [RequireOwner]
        private async Task RemoveBlackList(string name)
        {
            if (m_blacklistedUsers.ContainsKey(name))
                m_blacklistedUsers.Remove(name);
            await Context.Channel.SendMessageAsync($"Ok");
        }

        [Command("quit")]
        [Summary("")]
        [Hidden]
        [RequireOwner]
        private async Task Quit()
        {
            await Dolores.m_Instance.SaveState();
        }
    }
}
