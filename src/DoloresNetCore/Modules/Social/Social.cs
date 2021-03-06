﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Dolores.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Social
{
    [RequireInstalled]
    [LangSummary(LanguageDictionary.Language.PL, "Dodaje możliwość \"interakcji\" z użytkownikami, jak narazię jest to tylko dodawanie reakcji do wiadomości wskazanych osób")]
    [LangSummary(LanguageDictionary.Language.EN, "Adds \"interactions\" with users, so far it's only adding reactions to configured users")]
    public class Social : ModuleBase
    {
        private DiscordSocketClient m_Client;
        IServiceProvider m_Map;
        //Reactions m_Reactions;

        public Social(IServiceProvider map)
        {
            m_Map = map;
            m_Client = map.GetService<DiscordSocketClient>();
            //m_Reactions = map.GetService<Reactions>();
        }

        [Command("reactToMe")]
        [Summary("")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
        public async Task ReactToMe(params string[] messages)
        {
            List<string> reactions = new List<string>();
            foreach (var message in messages)
            {
                reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            //m_Reactions.Add(Context.User.Id, reactions.ToArray());

            await Context.Message.DeleteAsync();
        }

        [Command("stopReactToMe")]
        [Summary("")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
        public async Task StopReactToMe(params string[] messages)
        {
            //if (!messages.Any())
                //m_Reactions.ClearUser(Context.User.Id);
            //else
            //{
                List<string> reactions = new List<string>();
                foreach (var message in messages)
                {
                    reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
                }

                //m_Reactions.Remove(Context.User.Id, reactions.ToArray());
            //}

            await Context.Message.DeleteAsync();
        }

        [Command("reactTo")]
        [Summary("")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
        [Hidden]
        [RequireOwner]
        public async Task ReactTo(IUser user, params string[] messages)
        {
            List<string> reactions = new List<string>();
            foreach (var message in messages)
            {
                reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            //m_Reactions.Add(user.Id, reactions.ToArray());

            await Context.Message.DeleteAsync();
        }

        [Command("stopReactTo")]
        [Summary("")]
        [LangSummary(LanguageDictionary.Language.PL, "")]
        [LangSummary(LanguageDictionary.Language.EN, "")]
        [Hidden]
        [RequireOwner]
        public async Task StopReactTo(IUser user, params string[] messages)
        {
            //if (!messages.Any())
                //m_Reactions.ClearUser(user.Id);
            //else
            //{
                List<string> reactions = new List<string>();
                foreach (var message in messages)
                {
                    reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
                }

                //m_Reactions.Remove(user.Id, reactions.ToArray());
            //}

            await Context.Message.DeleteAsync();
        }
    }
}
