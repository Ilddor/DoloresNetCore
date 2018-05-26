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
    [LangSummary(LanguageDictionary.Language.PL, "Domyślny moduł z zestawem podstawowych komend")]
    [LangSummary(LanguageDictionary.Language.EN, "Default module with basic set of commands")]
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

		static private string GetModuleCommandsFormatted(ModuleInfo module, Configurations.GuildConfig guildConfig, Func<Attribute, bool> languageSummaryLambda)
		{
			string message = "";
			foreach (var it in module.Commands)
			{
				if (!it.Preconditions.Any(x => x is HiddenAttribute))
				{
					message += $"-`{guildConfig.Prefix}{it.Aliases.First()}`    - ";
					if (it.Attributes.Any(languageSummaryLambda))
						message += (it.Attributes.Where(languageSummaryLambda).First() as LangSummaryAttribute).Summary;
					message += "\n";
				}
			}

			return message;
		}

		static public Embed BuildHelpPage(int page, Configurations.GuildConfig guildConfig, CommandService commands, IServiceProvider map, ICommandContext context)
		{
			var random = new Random();
			Func<Attribute, bool> languageSummaryLambda =
				(Attribute x) =>
				{
					return x.GetType() == typeof(LangSummaryAttribute) && (x as LangSummaryAttribute).Lang == guildConfig.Lang;
				};

			var embedMessage = new EmbedBuilder().WithColor(random.Next(255), random.Next(255), random.Next(255));
			embedMessage.WithTitle($"{guildConfig.Translation.AvailableCommands}:\n");

			var modules = commands.Modules.Where(module =>
				{
					bool showIt = true;
					showIt &= !module.Preconditions.Any(precond => precond is HiddenAttribute);
					if (module.Preconditions.Any(precond => precond is RequireInstalledAttribute))
						showIt &= (module.Preconditions.Where(precond => precond is RequireInstalledAttribute).First().CheckPermissionsAsync(context, module.Commands.First(), map)).Result.IsSuccess;

					return showIt;
				});

			string message = "";
			if (page >= 0)
			{
				if (page >= modules.Count())
					page--;

				var module = modules.ElementAt(page);
				message = GetModuleCommandsFormatted(module, guildConfig, languageSummaryLambda);
				if (message != "")
					embedMessage.AddField(module.Name, message);

				embedMessage.Description += $"{page}/{modules.Count()-1}";
			}
			else
			{
				foreach (var module in modules)
				{
					message = "";
					message = GetModuleCommandsFormatted(module, guildConfig, languageSummaryLambda);
					if (message != "")
						embedMessage.AddField(module.Name, message);
				}
			}

			message = "";
			var uptime = DateTime.Now - Dolores.m_Instance.m_StartTime;
			message += $"{guildConfig.Translation.TimeOnline}: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m\n";
			message += $"{guildConfig.Translation.Version}: {Dolores.m_Instance.m_Version}\n";
			embedMessage.WithFooter(message);
			return embedMessage.Build();
		}

        [Command("help")]
        [Alias("analiza")]
        [LangSummary(LanguageDictionary.Language.PL, "Wyświetla ten tekst, dodając komendę jako parametr wyświetli dokładniejszy opis lub użycie")]
        [LangSummary(LanguageDictionary.Language.EN, "Shows this text, when you add command as a parameter it should show more detailed description of a command")]
        public async Task Help(params string[] command)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);


            Func<Attribute, bool> languageSummaryLambda =
                (Attribute x) =>
                {
                    return x.GetType() == typeof(LangSummaryAttribute) && (x as LangSummaryAttribute).Lang == guildConfig.Lang;
                };

            if (!command.Any())
            {
				guildConfig.LastHelpMessageId = (await Context.Channel.SendMessageAsync("", embed: BuildHelpPage(-1, guildConfig, m_Commands, m_Map, Context))).Id;
            }
			else if(command.Length == 1 && int.TryParse(command[0], out int pageNum))
			{
				var helpMessage = await Context.Channel.SendMessageAsync("", embed: BuildHelpPage(pageNum, guildConfig, m_Commands, m_Map, Context));
				guildConfig.LastHelpMessageId = helpMessage.Id;
				helpMessage.AddReactionAsync(new Emoji("⏮"));
				helpMessage.AddReactionAsync(new Emoji("⏭"));
			}
            else
            {
                var combinedCommand = string.Join(" ", command).ToLower();
                if(m_Commands.Commands.Any(x => x.Aliases.Contains(combinedCommand)))
                {
                    var commandInfo = m_Commands.Commands.Where(x => x.Aliases.Contains(combinedCommand)).First();
                    var embedMessage = new EmbedBuilder().WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255));

                    string title = $"`{guildConfig.Prefix}{combinedCommand}` ";

                    foreach(var parameter in commandInfo.Parameters)
                    {
                        if (parameter.IsOptional)
                            title += "[";
                        title += $"{parameter.Name}";
                        if (parameter.IsOptional)
                            title += "]";
                        title += " ";

                        string parameterName = parameter.Name;
                        if (parameter.IsOptional)
                            parameterName += $" ({guildConfig.Translation.Optional})";

                        string description = "";
                        if (parameter.Attributes.Any(languageSummaryLambda))
                            description += (parameter.Attributes.Where(languageSummaryLambda).First() as LangSummaryAttribute).Summary + "\n";
                        else
                            description += "No description";

                        if(parameter.Type.IsEnum)
                        {
                            description += $"{guildConfig.Translation.PossibleValues}:\n";
                            foreach(var value in Enum.GetValues(parameter.Type))
                            {
                                description += $" - {value}\n";
                            }
                        }

                        embedMessage.AddField(parameterName, description);
                    }

                    embedMessage.WithTitle(title);

                    string message = "";
                    if (commandInfo.Attributes.Any(languageSummaryLambda))
                        message += (commandInfo.Attributes.Where(languageSummaryLambda).First() as LangSummaryAttribute).Summary;
                    if (message != "")
                        embedMessage.WithDescription(message);

					guildConfig.LastHelpMessageId = (await Context.Channel.SendMessageAsync("", embed: embedMessage.Build())).Id;
                }
            }
			guildConfig.LastHelpCommandContext = Context;

			configs.SetGuildConfig(Context.Guild.Id, guildConfig);
        }

        [Command("ping")]
        [LangSummary(LanguageDictionary.Language.PL, "Umożliwia sprawdzenie działania bota")]
        [LangSummary(LanguageDictionary.Language.EN, "Checks if bot works")]
        public async Task Ping()
        {
            var client = m_Map.GetService<DiscordSocketClient>();
            await Context.Channel.SendMessageAsync("", embed:
                new EmbedBuilder()
                    .WithDescription("Pong!")
                    .AddField("Latency:", $"{client.Latency} ms")
                    .WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255))
					.Build());
        }

        [Command("invite")]
        [LangSummary(LanguageDictionary.Language.PL, "Link do zaproszenia bota na swój serwer")]
        [LangSummary(LanguageDictionary.Language.EN, "Link to invite bot to own server")]
        public async Task Invite()
        {
            var client = m_Map.GetService<DiscordSocketClient>();
            await Context.Channel.SendMessageAsync("", embed:
                new EmbedBuilder()
                    .WithTitle("InviteMe!")
                    .WithUrl("https://discordapp.com/oauth2/authorize?client_id=274940517735858176&scope=bot&permissions=255323384")
                    .WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255))
					.Build());
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
            string message = $"{guildConfig.Translation.Results}:\n";
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
        [RequireUserPermission(ChannelPermission.ManageMessages)]
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
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);
            if (!guildConfig.NSFWCHannelId.HasValue)
                return;

            var channel = client.GetChannel(guildConfig.NSFWCHannelId.Value) as ITextChannel;

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
            } while (guildConfig.BannedSubreddits.Contains(subreddit));
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
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            guildConfig.BannedSubreddits.Ban(subreddit);

            await Context.Message.DeleteAsync();

            var client = m_Map.GetService<DiscordSocketClient>();
            if (!guildConfig.NSFWCHannelId.HasValue)
                return;

            var channel = client.GetChannel(guildConfig.NSFWCHannelId.Value) as ITextChannel;
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
