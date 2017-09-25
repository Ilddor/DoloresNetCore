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
using Dolores.DataClasses;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Dolores.Modules.Misc
{
    public class Misc : ModuleBase
    {
        IServiceProvider m_Map;
        CommandService m_Commands;
        private Random m_Random = new Random();

        public Misc(CommandService commands, IServiceProvider map)
        {
            m_Map = map;
            m_Commands = commands;
        }

        [Command("help")]
        [Alias("analiza")]
        [Summary("Wyświetla ten tekst, dodając komendę jako parametr wyświetli dokładniejszy opis lub użycie")]
        public async Task Help(string command = null)
        {
            if (command == null)
            {
                string message = "Dostępne komendy:\n";
                foreach (var it in m_Commands.Commands)
                {
                    if (!it.Preconditions.Any(x => x is HiddenAttribute))
                        message += $" -`!{it.Name}`    - {it.Summary}\n";
                }
                message += $"\n\n";
                var uptime = DateTime.Now - Dolores.m_Instance.m_StartTime;
                message += $"Czas online(bota): {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m\n";
                message += $"Wersja: {Dolores.m_Instance.m_Version}\n";
                await Context.Channel.SendMessageAsync(message);
            }
            else
            {
                if(m_Commands.Commands.Any(x => x.Name == command))
                {
                    var commandInfo = m_Commands.Commands.Where(x => x.Name == command).First();
                    await Context.Channel.SendMessageAsync($"`{commandInfo.Name}` - {commandInfo.Summary}\n{commandInfo.Remarks}");
                }
            }
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

        [Command("notifyMe")]
        [Summary("Wysyła jednorazowo notyfikację gdy poszukiwana osoba pojawi się online")]
        [Remarks("W parametrze należy podać osobę o której chce się dostać notyfikację. Parametr musi używać \"Wzmianki\"")]
        private async Task NotifyMe(IGuildUser notify)
        {
            var notifications = m_Map.GetService<Notifications>();
            notifications.AddNotification(Context.User.Id, notify.Id);
            await Context.Message.DeleteAsync();
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
                    if ((it.Author.Id == m_Map.GetService<DiscordSocketClient>().CurrentUser.Id) || allUsers)
                    {
                        await it.DeleteAsync();
                    }
                }
            });
        }

        [Command("nsfw", RunMode = RunMode.Async)]
        [Alias("bodziuBajt")]
        [Summary("")]
        [Hidden]
        [OwnerOrBodziu]
        public async Task NSFW()
        {
            Random random = new Random();
            var client = m_Map.GetService<DiscordSocketClient>();
            var channel = client.GetChannel(272419366744883200) as ITextChannel;

            BannedSubreddits banned = m_Map.GetService<BannedSubreddits>();

            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            var webClient = new HttpClient(handler);
            //HttpResponseMessage page = await webClient.GetAsync($"https://www.reddit.com/r/{subreddits[random.Next(subreddits.Count)]}/new.json?sort=popular&limit=5");
            HttpResponseMessage page;
            string subreddit;
            bool isBanned = false;
            do
            {
                page = await webClient.GetAsync($"https://www.reddit.com/r/randnsfw/new.json?sort=popular&limit=5");
                subreddit = page.RequestMessage.RequestUri.LocalPath;
                subreddit = Regex.Match(subreddit, "/r/(.*)/").Groups[1].Value;
            } while (banned.Contains(subreddit));
            var reader = new StreamReader(await page.Content.ReadAsStreamAsync());
            JToken tmp = JsonConvert.DeserializeObject<JToken>(await reader.ReadToEndAsync());
            foreach(var child in tmp["data"]["children"])
            {
                await channel.SendMessageAsync($"Subreddit: {subreddit} {child["data"]["url"]}");
            }
            await Context.Message.DeleteAsync();
        }

        [Command("banSubreddit", RunMode = RunMode.Async)]
        [Summary("")]
        [Hidden]
        [OwnerOrBodziu]
        public async Task BanSubreddit(string subreddit)
        {
            BannedSubreddits banned = m_Map.GetService<BannedSubreddits>();

            banned.Ban(subreddit);

            await Context.Message.DeleteAsync();

            var client = m_Map.GetService<DiscordSocketClient>();
            var channel = client.GetChannel(272419366744883200) as ITextChannel;
            var messages = channel.GetMessagesAsync(5);
            await messages.ForEachAsync(async x =>
            {
                foreach (var it in x)
                {
                    if(it.Content.Contains(subreddit))
                        it.DeleteAsync();
                }
            });
        }
    }
}
