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
        private IServiceProvider m_Map;
        private CommandService m_Commands;
        private Random m_Random = new Random();

        public Misc(CommandService commands, IServiceProvider map)
        {
            m_Map = map;
            m_Commands = commands;
        }

        [Command("help")]
        [Alias("analiza")]
        [LangSummary(LanguageDictionary.Language.PL, "Wyświetla ten tekst, dodając komendę jako parametr wyświetli dokładniejszy opis lub użycie")]
        [LangSummary(LanguageDictionary.Language.EN, "Shows this text, when you add command as a parameter it should show more detailed description of a command")]
        public async Task Help(string command = null)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            Func<Attribute, bool> languageSummaryLambda =
                (Attribute x) =>
                {
                    return x.GetType() == typeof(LangSummaryAttribute) && (x as LangSummaryAttribute).Lang == guildConfig.Lang;
                };

            if (command == null)
            {
                var embedMessage = new EmbedBuilder().WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255));
                embedMessage.WithTitle($"{LanguageDictionary.GetString(LanguageDictionary.LangString.AvailableCommands, guildConfig.Lang)}:\n");
                string message = "";
                foreach (var module in m_Commands.Modules)
                {
                    if(module.Preconditions.Any(x => x is RequireInstalledAttribute) &&
                       !(await module.Preconditions.Where(x => x is RequireInstalledAttribute).First().CheckPermissions(Context, module.Commands.First(), m_Map)).IsSuccess)
                        continue; // If module was not installed, omit it in help

                    message = "";
                    foreach (var it in module.Commands)
                    {
                        if (!it.Preconditions.Any(x => x is HiddenAttribute))
                        {
                            message += $" -`{guildConfig.Prefix}{it.Name}`    - ";
                            if (it.Attributes.Any(languageSummaryLambda))
                                message += (it.Attributes.Where(languageSummaryLambda).First() as LangSummaryAttribute).Summary;
                            message += "\n";
                        }
                    }
                    if(message != "")
                        embedMessage.AddField(module.Name, message);
                }

                message = "";
                var uptime = DateTime.Now - Dolores.m_Instance.m_StartTime;
                message += $"{LanguageDictionary.GetString(LanguageDictionary.LangString.TimeOnline, guildConfig.Lang)}: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m\n";
                message += $"{LanguageDictionary.GetString(LanguageDictionary.LangString.Version, guildConfig.Lang)}: {Dolores.m_Instance.m_Version}\n";
                embedMessage.WithFooter(message);
                await Context.Channel.SendMessageAsync("", embed: embedMessage);
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

        [Command("setLang")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić język jakim będzie posługiwać się bot na tym serwerze")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set language in which bot will operate on this server")]
        [RequireAdministrator]
        public async Task SetLang(LanguageDictionary.Language lang)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            guildConfig.Lang = lang;

            configs.SetGuildConfig(Context.Guild.Id, guildConfig);
        }

        [Command("setPrefix")]
        [LangSummary(LanguageDictionary.Language.PL, "Pozwala ustawić jakiego prefixu będzie nasłuchiwać bot")]
        [LangSummary(LanguageDictionary.Language.EN, "This allows you to set bot prefix to use commands")]
        [RequireAdministrator]
        public async Task SetPrefix(string prefix)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            guildConfig.Prefix = prefix;

            configs.SetGuildConfig(Context.Guild.Id, guildConfig);
        }

        [Command("ping")]
        [LangSummary(LanguageDictionary.Language.PL, "Umożliwia sprawdzenie działania bota")]
        [LangSummary(LanguageDictionary.Language.EN, "Checks if bot works")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription("Pong!").WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255)));
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
        [LangSummary(LanguageDictionary.Language.PL, "Losuje liczbę z podanego przedziału (100 jeśli nie zdefiniowano)")]
        [LangSummary(LanguageDictionary.Language.EN, "Draws a number from the given range (100 default)")]
        private async Task Roll(int max = 100)
        {
            await Context.Channel.SendMessageAsync($"{Context.User.Username}: {m_Random.Next(0, max)}");
        }

        [Command("rollChannel")]
        [LangSummary(LanguageDictionary.Language.PL, "Losuje liczby dla całego kanału głosowego oraz wyświetla je w kolejności malejącej")]
        [LangSummary(LanguageDictionary.Language.EN, "Draws random numbers for each person in your voice channel and shows it in descending order")]
        private async Task RollChannel()
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);
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
            string message = $"{LanguageDictionary.GetString(LanguageDictionary.LangString.Results, guildConfig.Lang)}:\n";
            foreach (var it in results.OrderByDescending(x => x.Key))
            {
                message += $"{it.Value}: {it.Key}\n";
            }
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("notifyMe")]
        [LangSummary(LanguageDictionary.Language.PL, "Wysyła jednorazowo notyfikację gdy poszukiwana osoba pojawi się online")]
        [LangSummary(LanguageDictionary.Language.EN, "Sends one time notification when a searched person comes online")]
        [Remarks("W parametrze należy podać osobę o której chce się dostać notyfikację. Parametr musi używać \"Wzmianki\"")]
        private async Task NotifyMe(IGuildUser notify)
        {
            var notifications = m_Map.GetService<Notifications>();
            notifications.AddNotification(Context.User.Id, notify.Id);
            await Context.Message.DeleteAsync();
        }

        [Command("removeHistory")]
        [LangSummary(LanguageDictionary.Language.PL, "Usuwa historie n wiadomości")]
        [LangSummary(LanguageDictionary.Language.EN, "Removes history of n messages")]
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
            List<IMessage> forDelete = new List<IMessage>();
            await messages.ForEachAsync(async x =>
            {
                foreach (var it in x)
                {
                    if ((it.Author.Id == m_Map.GetService<DiscordSocketClient>().CurrentUser.Id) || allUsers)
                    {
                        forDelete.Add(it);
                    }
                }
            });
            channel.DeleteMessagesAsync(forDelete);
        }

        [Command("nsfw", RunMode = RunMode.Async)]
        [Alias("bodziuBajt")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
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
                channel.SendMessageAsync($"Subreddit: {subreddit} {child["data"]["url"]}");
            }
            await Context.Message.DeleteAsync();
        }

        [Command("banSubreddit", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
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
            List<IMessage> forDelete = new List<IMessage>();
            await messages.ForEachAsync(async x =>
            {
                foreach (var it in x)
                {
                    if (it.Content.Contains(subreddit))
                        forDelete.Add(it);
                }
            });

            channel.DeleteMessagesAsync(forDelete);
        }
    }
}
