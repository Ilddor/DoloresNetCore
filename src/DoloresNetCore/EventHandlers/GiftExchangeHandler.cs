using Discord;
using Discord.WebSocket;
using Dolores.DataClasses;
using Dolores.Modules.Voice;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.EventHandlers
{
    public class GiftExchangeHandler : IInstallable
    {
        private DiscordSocketClient m_Client;
        private IServiceProvider m_Map;

        public Task Install(IServiceProvider map)
        {
            m_Client = map.GetService<DiscordSocketClient>();
            m_Map = map;
            m_Client.ReactionAdded += ReactionAdded;

            return Task.CompletedTask;
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            var exchanges = m_Map.GetService<Exchanges>();
            if (exchanges.IsExchange(message.Id) && !exchanges.IsRolled(message.Id))
            {
                await UpdateHelperMessage(message.Id, m_Map);
            }
        }

        static public async Task UpdateHelperMessage(ulong exchangeID, IServiceProvider map)
        {
            var exchanges = map.GetService<Exchanges>();

            var guild = map.GetService<DiscordSocketClient>().GetGuild(exchanges.GetGuildID(exchangeID));
            var messageChannel = guild.GetTextChannel(exchanges.GetChannelID(exchangeID));
            var random = new Random();

            var configs = map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(exchanges.GetGuildID(exchangeID));

            string signInReaction = exchanges.GetSignInReaction(exchangeID);
            var exchangeSignPost = await messageChannel.GetMessageAsync(exchangeID) as IUserMessage;

            var signedUsersAsync = exchangeSignPost.GetReactionUsersAsync(Emote.Parse(signInReaction), 10000);
			var signedUsers = signedUsersAsync.Flatten();

            var gatheredAddresses = new EmbedBuilder().WithColor(random.Next(255), random.Next(255), random.Next(255));

            gatheredAddresses.WithAuthor(exchangeSignPost.Author);
            gatheredAddresses.WithDescription(
                guildConfig.Translation.OrganizationalMessageFullPart1 + $"`{guildConfig.Prefix}setExchangeAddress " + exchangeID + 
                guildConfig.Translation.OrganizationalMessageFullPart2 + $"`{guildConfig.Prefix}myExchangePictures " + exchangeID + " imgur url`");

            string withAddress = "";
            string withoutAddress = "";
			//foreach (var user in signedUsers)
			System.Linq.AsyncEnumerable.Do(signedUsers, user =>
			{
				if (exchanges.GetUserAddress(exchangeID, user.Id) != null)
				{
					withAddress += $"- {user.Mention}\n";
				}
				else
				{
					withoutAddress += $"- {user.Mention}\n";
				}
			});

            if (withAddress == "")
                withAddress = guildConfig.Translation.Missing;
            if (withoutAddress == "")
                withoutAddress = guildConfig.Translation.Missing;

            gatheredAddresses.AddField(guildConfig.Translation.MissingAddresses, withoutAddress);
            gatheredAddresses.AddField(guildConfig.Translation.FilledAddresses, withAddress);
            gatheredAddresses.AddField(guildConfig.Translation.ExchangeRolled,
                exchanges.IsRolled(exchangeID) ? guildConfig.Translation.Yes : guildConfig.Translation.No);

            if (exchanges.AnyPictureUrls(exchangeID))
            {
                string urls = "";
				//foreach (var user in signedUsers)
				System.Linq.AsyncEnumerable.Do(signedUsers, user =>
				{
					if (exchanges.GetUserPicturesUrl(exchangeID, user.Id) != null)
					{
						urls += $"- {user.Mention}: {exchanges.GetUserPicturesUrl(exchangeID, user.Id)}\n";
					}
				});

                gatheredAddresses.AddField(guildConfig.Translation.Pictures, urls);
            }

            var helperMessage = await messageChannel.GetMessageAsync(exchanges.GetHelperMessageID(exchangeID));
            await (helperMessage as IUserMessage).ModifyAsync(x =>
            {
                x.Content = "";
                x.Embed = gatheredAddresses.Build();
            });
        }
    }
}
