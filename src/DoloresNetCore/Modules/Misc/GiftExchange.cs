using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.CustomAttributes;
using Dolores.DataClasses;
using Dolores.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.Modules.Misc
{
    //[RequireInstalled]
    [LangSummary(LanguageDictionary.Language.PL, "brak opisu")]
    [LangSummary(LanguageDictionary.Language.EN, "no description")]
    public class GiftExchange : ModuleBase
    {
        private IServiceProvider m_Map;
        private Random m_Random = new Random();

        public GiftExchange(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("setExchange")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
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
