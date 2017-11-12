using Discord.WebSocket;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dolores.EventHandlers
{
    class StatsUpdateHandler : IInstallable
    {
        private DiscordSocketClient m_Client;
        IServiceProvider m_Map;
        struct Stats
        {
            public int server_count { get; set; }
        }

        public Task Install(IServiceProvider map)
        {
            m_Map = map;
            m_Client = m_Map.GetService<DiscordSocketClient>();

            //m_Client.JoinedGuild += JoinedGuild;
            //m_Client.LeftGuild += LeftGuild;
            //m_Client.GuildAvailable += GuildAvailable;

            return Task.CompletedTask;
        }

        private Task JoinedGuild(SocketGuild arg)
        {
            throw new NotImplementedException();
        }

        private Task LeftGuild(SocketGuild arg)
        {
            throw new NotImplementedException();
        }

        private Task GuildAvailable(SocketGuild arg)
        {
            UpdateBotStats();

            return Task.CompletedTask;
        }

        private async void UpdateBotStats()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", m_Map.GetService<APIKeys>().BotsDiscordAPI);
            int numGuilds = m_Client.Guilds.Count;

            StringContent content = new StringContent(JsonConvert.SerializeObject(new Stats { server_count = m_Client.Guilds.Count }));
            content.Headers.TryAddWithoutValidation("Authorization", m_Map.GetService<APIKeys>().BotsDiscordAPI);

            var response = await httpClient.PostAsync($"https://bots.discord.pw/api/bots/274940517735858176/stats", content);
            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //var result = JsonConvert.DeserializeObject<StatsResponse>(responseData);
        }
    }
}
