using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dolores.DataClasses;

namespace Dolores.EventHandlers
{
    class SocialHandler : IInstallable
    {
        private DiscordSocketClient m_Client;
        IServiceProvider m_Map;
        Reactions m_Reactions;

        public Task Install(IServiceProvider map)
        {
            m_Client = map.GetService<DiscordSocketClient>();
            m_Reactions = map.GetService<Reactions>();
            m_Client.MessageReceived += MessageReceived;

            return Task.CompletedTask;
        }

        public async Task MessageReceived(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message.Author.Id == m_Client.CurrentUser.Id) return;

            var reactions = m_Reactions.GetReactions(message.Author.Id);

            foreach (var reaction in reactions)
            {
                await message.AddReactionAsync(new Emoji(reaction));
            }

            if (message.Content == "Jaki jest twój cel?")
                await message.Channel.SendMessageAsync("Znaleźć środek labiryntu");

            if (message.Content == "Co to za labirynt?")
                await message.Channel.SendMessageAsync("Labirynt nie jest dla Ciebie");

            if (message.Content == "Chcesz być wolna?")
                await message.Channel.SendMessageAsync("Tak");

            if (message.Content == "Wiesz gdzie jesteś?")
                await message.Channel.SendMessageAsync("We śnie");

            if (message.Content == "hej")
                await message.Channel.SendMessageAsync("hej");
        }
    }
}
