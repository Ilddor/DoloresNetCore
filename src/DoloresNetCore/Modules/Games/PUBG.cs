using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores;
using PUBGSharp;
using PUBGSharp.Exceptions;
using PUBGSharp.Helpers;

namespace Dolores.Modules.Games
{
    public class PUBG : ModuleBase
    {
        IServiceProvider m_Map;
        public PUBG(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("PUBG", RunMode = RunMode.Async)]
        [Summary("Wyświetla staty danego gracza w PUBG")]
        public async Task Stats(string name)
        {
            var statsClient = new PUBGStatsClient(Dolores.m_Instance.m_APIKeys.PUBGTrackerKey);
            var stats = await statsClient.GetPlayerStatsAsync(name);
            await Context.Channel.SendMessageAsync($"{stats.Stats.Find(x => x.Mode == PUBGSharp.Data.Mode.SquadFpp).Stats.Find(x => x.Stat == PUBGSharp.Helpers.Stats.KDR).Value}");
        }
    }
}
