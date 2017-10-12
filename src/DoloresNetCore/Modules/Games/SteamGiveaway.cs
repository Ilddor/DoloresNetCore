﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Dolores.DataClasses;
using Dolores.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Games
{
    [RequireInstalled]
    [LangSummary(LanguageDictionary.Language.PL, "Mechanizm losowania kluczy wśród ludzi zgłoszonych z serwera")]
    [LangSummary(LanguageDictionary.Language.EN, "Raffle mechanism to draw game key for current server users signed for raffle")]
    public class SteamGiveaway : ModuleBase
    {
        IServiceProvider m_Map;
        private Random m_Random = new Random();
        public SteamGiveaway(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("key")]
        [LangSummary(LanguageDictionary.Language.PL, "Pisząc tę komendę w prywatnej wiadomości i podając jako parametr klucz gry możesz dodać do bazy bota klucz który rozlosuje wśród zgłoszonych użytkowników")]
        [LangSummary(LanguageDictionary.Language.EN, "Sending this command as a private DM to the bot it will select random person from signed users that will receive game activation code provided as a parameter")]
        [RequireContext(ContextType.DM)]
        public async Task Key(string key)
        {
            SignedUsers signedUsers = m_Map.GetService<SignedUsers>();

            signedUsers.m_Mutex.WaitOne();
            int usersCount = signedUsers.m_Users.Count;
            ulong userId = 0;
            do
            {
                userId = signedUsers.m_Users.ElementAt(m_Random.Next(0, usersCount - 1)).Key;
            } while (userId == Context.User.Id);
            signedUsers.m_Mutex.ReleaseMutex();
            SocketGuild misiaki = m_Map.GetService<DiscordSocketClient>().GetGuild(269960016591716362);
            SocketGuildUser winningUser = misiaki.GetUser(userId);
            IDMChannel winnerChannel = await winningUser.GetOrCreateDMChannelAsync();
            await winnerChannel.SendMessageAsync($"Wygrałeś(aś) klucz podarowany przez: {Context.User.Mention} oto i on: {key}");
            await Context.Channel.SendMessageAsync($"Klucz wygrał(a): {winningUser.Mention} , udział brało {usersCount} użytkowników.");
            await misiaki.DefaultChannel.SendMessageAsync($"{winningUser.Mention} wygrał klucz zgłoszony przez {Context.User.Mention}, w zabawie brało udział tyle osób: {usersCount}. Gratulacje!");
        }

        [Command("keyChannel")]
        [LangSummary(LanguageDictionary.Language.PL, "Działa podobnie do komendy key z tą różnicą, że losuje klucz z osób znajdujących się na twoim kanale głosowym")]
        [LangSummary(LanguageDictionary.Language.EN, "Works similarly to key command but it will select random person from your voice channel")]
        [RequireContext(ContextType.DM)]
        public async Task KeyChannel(string key)
        {
            SignedUsers signedUsers = m_Map.GetService<SignedUsers>();

            ulong userId = 0;
            SocketGuild misiaki = m_Map.GetService<DiscordSocketClient>().GetGuild(269960016591716362);
            SocketGuildUser donor = misiaki.GetUser(Context.User.Id);
            int userCount = donor.VoiceChannel.Users.Count - 1;
            if (donor.VoiceChannel != null)
            {
                if (donor.VoiceChannel.Users.Count >= 2)
                {
                    do
                    {
                        userId = donor.VoiceChannel.Users.ElementAt(m_Random.Next(0, userCount)).Id;
                    } while (userId == Context.User.Id);
                }
            }

            SocketGuildUser winningUser = misiaki.GetUser(userId);
            IDMChannel winnerChannel = await winningUser.GetOrCreateDMChannelAsync();
            await winnerChannel.SendMessageAsync($"Wygrałeś(aś) klucz podarowany przez: {Context.User.Mention} oto i on: {key}");
            await Context.Channel.SendMessageAsync($"Klucz wygrał(a): {winningUser.Mention} , udział brało {userCount} użytkowników.");
            await misiaki.DefaultChannel.SendMessageAsync($"Spośród osób na kanale głosowym {winningUser.Mention} wygrał klucz zgłoszony przez {Context.User.Mention}, w zabawie brało udział tyle osób: {userCount}. Gratulacje!");
        }

        [Command("listKey")]
        [LangSummary(LanguageDictionary.Language.PL, "Wpisuje liste osób zapisanych do losowania kluczy")]
        [LangSummary(LanguageDictionary.Language.EN, "Prints out list of users signed for a key raffles")]
        public async Task ListKey()
        {
            SignedUsers signedUsers = m_Map.GetService<SignedUsers>();
            SocketGuild misiaki = m_Map.GetService<DiscordSocketClient>().GetGuild(269960016591716362);
            string message = "Zapisani: ";
            signedUsers.m_Mutex.WaitOne();
            try
            {
                foreach(var id in signedUsers.m_Users)
                {
                    message += $" {misiaki.GetUser(id.Key).Mention}";
                }
            }
            catch (Exception) { }
            signedUsers.m_Mutex.ReleaseMutex();
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("signKey")]
        [LangSummary(LanguageDictionary.Language.PL, "Wpisując tę komendę zapisujesz się do grupy osób wśród której rozlosowywany będzie klucz gry jeśli ktoś takowy zgłosi botowi")]
        [LangSummary(LanguageDictionary.Language.EN, "Signs you to group for a game key raffle if someone will send key to the bot")]
        public async Task SignKey(string mention = null)
        {
            IGuildUser user = Context.User as IGuildUser;
            if (user.Username == "Ilddor" && mention != null)
            {
                user = await Context.Guild.GetUserAsync(ulong.Parse(mention.Replace("<@!","").Replace("<@","").Replace(">","")));
            }
            var roles = user.RoleIds;
            ulong role = 273446118405177345; // Misiaki
            //SocketUser user = Context.Guild.GetUserAsync()
            if (roles.Contains(role))
            {
                SignedUsers signedUsers = m_Map.GetService<SignedUsers>();
                signedUsers.m_Mutex.WaitOne();
                bool added = true;
                try
                {
                    if (!signedUsers.m_Users.ContainsKey(user.Id))
                        signedUsers.m_Users.Add(user.Id, true);
                    else
                        added = false;
                }
                catch (Exception) { }
                signedUsers.m_Mutex.ReleaseMutex();
                if(added)
                    await Context.Channel.SendMessageAsync($"{user.Mention} zostałeś dodany na listę, jeśli kiedyś wygrasz klucz to otrzymasz go w prywatnej wiadomości");
                else
                    await Context.Channel.SendMessageAsync($"{user.Mention} już jesteś na liście chętnych na klucze, nie przesadzaj");
            }
        }
    }
}
