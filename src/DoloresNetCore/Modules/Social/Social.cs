using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons;
using Dolores.DataClasses;
using Dolores.CustomAttributes;

namespace Dolores.Modules.Social
{
    public class Social : ModuleBase
    {
        private DiscordSocketClient m_Client;
        IDependencyMap m_Map;
        Reactions m_Reactions;

        public Social(IDependencyMap map)
        {
            m_Map = map;
            m_Client = map.Get<DiscordSocketClient>();
            map.TryGet<Reactions>(out m_Reactions);
        }

        public void Install(IDependencyMap map)
        {
            m_Client = map.Get<DiscordSocketClient>();
            map.TryGet<Reactions>(out m_Reactions);
            m_Client.MessageReceived += MessageReceived;
        }

        [Command("reactToMe")]
        [Summary("")]
        [Hidden]
        public async Task ReactToMe(params string[] messages)
        {
            List<string> reactions = new List<string>();
            foreach (var message in messages)
            {
                reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            m_Reactions.Add(Context.User.Id, reactions.ToArray());

            await Context.Message.DeleteAsync();
        }

        [Command("stopReactToMe")]
        [Summary("")]
        [Hidden]
        public async Task StopReactToMe(params string[] messages)
        {
            if (!messages.Any())
                m_Reactions.ClearUser(Context.User.Id);
            else
            {
                List<string> reactions = new List<string>();
                foreach (var message in messages)
                {
                    reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
                }

                m_Reactions.Remove(Context.User.Id, reactions.ToArray());
            }

            await Context.Message.DeleteAsync();
        }

        [Command("reactTo")]
        [Summary("")]
        [Hidden]
        [RequireOwner]
        public async Task ReactTo(IUser user, params string[] messages)
        {
            List<string> reactions = new List<string>();
            foreach (var message in messages)
            {
                reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            m_Reactions.Add(user.Id, reactions.ToArray());

            await Context.Message.DeleteAsync();
        }

        [Command("stopReactTo")]
        [Summary("")]
        [Hidden]
        [RequireOwner]
        public async Task StopReactTo(IUser user, params string[] messages)
        {
            if (!messages.Any())
                m_Reactions.ClearUser(user.Id);
            else
            {
                List<string> reactions = new List<string>();
                foreach (var message in messages)
                {
                    reactions.AddRange(message.Split(new string[] { ">", "<" }, options: StringSplitOptions.RemoveEmptyEntries).ToList());
                }

                m_Reactions.Remove(user.Id, reactions.ToArray());
            }

            await Context.Message.DeleteAsync();
        }

        public async Task MessageReceived(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message.Author.Id == m_Client.CurrentUser.Id) return;
            /*if (message.Author.Id == 132131643849834497) // Bodziu messages reactions:D
            {
                SocketGuild misiaki = m_Map.Get<DiscordSocketClient>().GetGuild(269960016591716362);
                await message.AddReactionAsync($"Polandball:272424031892930561");
                await message.AddReactionAsync($"Bodzia:272421593416990728");
            }

            if (message.Author.Id == 131816357980405760)
            {
                SocketGuild misiaki = m_Map.Get<DiscordSocketClient>().GetGuild(269960016591716362);
                await message.AddReactionAsync($"Polandball:272424031892930561");
            }*/
            var reactions = m_Reactions.GetReactions(message.Author.Id);
            foreach (var reaction in reactions)
                await message.AddReactionAsync(reaction);

            if (message.Content == "Jaki jest twój cel?")
                await message.Channel.SendMessageAsync("Znaleźć środek labiryntu");

            if(message.Content == "Co to za labirynt?")
                await message.Channel.SendMessageAsync("Labirynt nie jest dla Ciebie");

            if(message.Content == "Chcesz być wolna?")
                await message.Channel.SendMessageAsync("Tak");

            if(message.Content == "Wiesz gdzie jesteś?")
                await message.Channel.SendMessageAsync("We śnie");

            if (message.Content == "hej")
                await message.Channel.SendMessageAsync("hej");
        }
    }
}
