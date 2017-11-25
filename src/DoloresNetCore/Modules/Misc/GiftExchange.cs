using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.CustomAttributes;
using Dolores.DataClasses;
using Dolores.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.Modules.Misc
{
    //[RequireInstalled]
    [LangSummary(LanguageDictionary.Language.PL, "Umożliwia tworzenie eventów polegających na wymianie okazjonjonalnej prezentów między osobami zapisanymi, zapisy odbywają się przez dodanie reakcji do wybranego wpisu")]
    [LangSummary(LanguageDictionary.Language.EN, "Allows to create events consisting of exchange of occasional gifts between registered users, the registration takes place by adding a reaction to the selected message.")]
    public class GiftExchange : ModuleBase
    {
        private IServiceProvider m_Map;
        private Random m_Random = new Random();

        public GiftExchange(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("setExchange")]
        [LangSummary(LanguageDictionary.Language.PL, "Ustawia wiadomość o podanym ID jako zapis do eventu wymiany, jako drugi parametr musi być podana reakcja służąca do zapisu")]
        [LangSummary(LanguageDictionary.Language.EN, "Sets message of given ID as a signup message for gift exchange event, as a second parameter there should be added a reaction that will sign users that use it")]
        [RequireAdministrator]
        public async Task SetExchange(ulong postID, string signInEmoji)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            await Context.Message.DeleteAsync();
            var exchangeSignPost = await Context.Channel.GetMessageAsync(postID) as IUserMessage;

            var exchanges = m_Map.GetService<Exchanges>();

            //var emote = Emote.Parse(signInEmoji);
            if (exchanges.IsExchange(postID))
                return;
            else
                exchanges.AddExchange(postID);

            var helperMessage = await Context.Channel.SendMessageAsync(guildConfig.Translation.OrganizationalMessage);

            exchanges.AddHelperMessage(postID, helperMessage.Id);
            exchanges.AddSignInReaction(postID, signInEmoji);
            exchanges.AddChannelID(postID, Context.Channel.Id);
            exchanges.AddGuildID(postID, Context.Guild.Id);

            await GiftExchangeHandler.UpdateHelperMessage(postID, m_Map);
        }

        [Command("rollExchange", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Losuje pary dla zapisanych osób oraz rozsyła odpowiednie adresy")]
        [LangSummary(LanguageDictionary.Language.EN, "Draws pairs for signed users and sends appropriate addresses")]
        [RequireAdministrator]
        public async Task RollExchange(ulong postID)
        {
            await Context.Message.DeleteAsync();

            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);

            var exchangeSignPost = await Context.Channel.GetMessageAsync(postID) as IUserMessage;

            var exchanges = m_Map.GetService<Exchanges>();

            //var emote = Emote.Parse(signInEmoji);
            if (!exchanges.IsExchange(postID))
                return;
            if (exchanges.IsRolled(postID))
                return;

            var signUpMessage = await Context.Channel.GetMessageAsync(postID) as IUserMessage;

            var signedUsers = await signUpMessage.GetReactionUsersAsync(exchanges.GetSignInReaction(postID));

            Dictionary<IUser, string> usersWithAddress = new Dictionary<IUser, string>();

            foreach(var user in signedUsers)
            {
                if(exchanges.GetUserAddress(postID, user.Id) != null)
                {
                    usersWithAddress.Add(user, exchanges.GetUserAddress(postID, user.Id));
                }
            }

            Dictionary<IUser, Tuple<IUser, string>> giftPairs = new Dictionary<IUser, Tuple<IUser, string>>();

            List<Tuple<IUser, string>> usersToDraw = new List<Tuple<IUser, string>>();

            bool cornerCaseRedoDraw;
            do
            {
                giftPairs.Clear();
                usersToDraw.Clear();
                cornerCaseRedoDraw = false;

                foreach (var user in usersWithAddress)
                {
                    usersToDraw.Add(new Tuple<IUser, string>(user.Key, user.Value));
                }

                foreach (var user in usersWithAddress)
                {
                
                    Tuple<IUser, string> drawnUser = null;
                    do
                    {
                        // In case we endup with last user to draw being the same we draw for - we need to redo whole draw
                        if (usersToDraw.Count == 1 && usersToDraw[0].Item1.Id == user.Key.Id)
                        {
                            cornerCaseRedoDraw = true;
                            break;
                        }

                        drawnUser = usersToDraw.ToArray()[m_Random.Next(usersToDraw.Count)];
                    }
                    while (user.Key.Id == drawnUser.Item1.Id);

                    usersToDraw.Remove(drawnUser);

                    giftPairs.Add(user.Key, drawnUser);
                }
            }
            while (cornerCaseRedoDraw);

            exchanges.SetRolled(postID);

            // Make security copy in case someone did not send gift or something else went wrong
            using (FileStream stream = File.Open($"exchange{postID}.dat", FileMode.Create))
            {
                var streamWriter = new StreamWriter(stream);
                foreach (var pair in giftPairs)
                {
                    streamWriter.WriteLine($"{pair.Key.Mention} - {pair.Value.Item1.Mention} : {pair.Value.Item2}");
                }
                streamWriter.Flush();
                stream.Flush();
            }

            foreach (var pair in giftPairs)
            {
                var dmChannel = await pair.Key.GetOrCreateDMChannelAsync();

                await dmChannel.SendMessageAsync(
                    guildConfig.Translation.GifterMessagePart1 +
                    Context.Guild.Name +
                    guildConfig.Translation.GifterMessagePart2 +
                    pair.Value.Item1.Username +
                    guildConfig.Translation.GifterMessagePart3 +
                    pair.Value.Item2 +
                    guildConfig.Translation.GifterMessagePart4);
            }

            await GiftExchangeHandler.UpdateHelperMessage(postID, m_Map);
        }


        [Command("setExchangeAddress")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
        [RequireContext(ContextType.DM)]
        public async Task SetExchangeAddress(ulong exchangeID, params string[] address_lines)
        {
            var exchanges = m_Map.GetService<Exchanges>();
            if (exchanges.IsExchange(exchangeID))
            {
                string addressCombined = string.Join(" ", address_lines);
                exchanges.AddAddress(exchangeID, Context.User.Id, addressCombined);

                await GiftExchangeHandler.UpdateHelperMessage(exchangeID, m_Map);

                var configs = m_Map.GetService<Configurations>();
                Configurations.GuildConfig guildConfig = configs.GetGuildConfig(exchanges.GetGuildID(exchangeID));

                await Context.Channel.SendMessageAsync(guildConfig.Translation.UserFilledAddressResponse);
            }
        }
    }
}
