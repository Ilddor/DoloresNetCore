using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.CustomAttributes;
using Dolores.DataClasses;
using Dolores.Modules.Voice;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dolores.Modules.Misc
{
    [RequireOwner]
    [Hidden]
    public class Administration : ModuleBase
    {
        private IServiceProvider m_Map;
        private Random m_Random = new Random();

        public Administration(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("listGuilds")]
        [Hidden]
        [RequireOwner]
        public async Task ListGuilds()
        {
            var client = m_Map.GetService<DiscordSocketClient>();
            string message = "";
            foreach (var guild in client.Guilds)
            {
                message += $"{guild.Id} - {guild.Name} : {guild.Users.Count}\n";
            }


            await Context.Channel.SendMessageAsync("Available guilds:", embed: new EmbedBuilder().WithDescription(message).WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255)));
        }

        [Command("whereAreYou")]
        [Hidden]
        [RequireOwner]
        public async Task WhereAreYou()
        {
            string message = "Not connected";
            Voice.Voice.AudioClientWrapper audioClient = m_Map.GetService<Voice.Voice.AudioClientWrapper>();
            if (audioClient.m_AudioClient != null)
            {
                if(audioClient.m_AudioClient.ConnectionState == ConnectionState.Connected)
                {
                    message = (audioClient.m_CurrentChannel as SocketVoiceChannel).Guild.Name;
                }
            }


            await Context.Channel.SendMessageAsync("I'm here:", embed: new EmbedBuilder().WithDescription(message).WithColor(m_Random.Next(255), m_Random.Next(255), m_Random.Next(255)));
        }
    }
}
