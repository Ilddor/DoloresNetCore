using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using Dolores.LeagueOfLegends.DataObjects.League;
using Dolores.LeagueOfLegends.DataObjects.Summoner;
using Dolores.LeagueOfLegends.DataObjects.CurrentGame;
using Dolores.LeagueOfLegends.DataObjects.StaticData;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Games
{
    public class RiotAPI : ModuleBase
    {
        IServiceProvider m_Map;
        Dictionary<ulong, string> m_SummonerNames;

        public class ChampionListWrapper
        {
            public ChampionList m_ChampionList;

            public ChampionListWrapper(string apiKey)
            {
                Task<ChampionList> task = APICalls.GetChampions(null, apiKey);
                task.Wait();
                m_ChampionList = task.Result;
            }

            private ChampionListWrapper() { }
        }

        ChampionListWrapper m_ChampionList;

        // Rework this function!
        private async Task GetChampionList(string apiKey)
        {
            //m_ChampionList = m_Map.GetService<ChampionListWrapper>();
            m_ChampionList = new ChampionListWrapper(apiKey);
            /*if(!m_Map.TryGet(out m_ChampionList))
            {
                m_ChampionList = await APICalls.GetChampions(null);
                m_Map.Add(m_ChampionList);
            }*/
        }

        public RiotAPI(IServiceProvider map)
        {
            m_Map = map;
            //TODO move it from here, make it changable and savable to file, make keys hidden in files
            m_SummonerNames = new Dictionary<ulong, string>();
            m_SummonerNames[131816357980405760] = "Ilddor";
            m_SummonerNames[191149407700385793] = "Wojtus";
            m_SummonerNames[248849249507213322] = "Nidamir";
            m_SummonerNames[132130373948669953] = "Lothek";
            m_SummonerNames[248117703972225027] = "kamilosxd678";
            m_SummonerNames[128147347183108097] = "PepegoPL";
            m_SummonerNames[144175864505040896] = "Borodein";
            m_SummonerNames[252033506941599744] = "ZdulaEzCebula";
        }

        [Command("showLore", RunMode = RunMode.Async)]
        [Summary("Wyświetla lore podanego bohatera z League of Legends")]
        public async Task ShowLore(string championName = null)
        {
            string apiKey = m_Map.GetService<APIKeys>().RiotAPIKey;
            await GetChampionList(apiKey);
            //m_ChampionList = await APICalls.GetChampions(null);

            if (m_ChampionList.m_ChampionList.Data.ContainsKey(championName))
            {
                await Context.Channel.SendMessageAsync(m_ChampionList.m_ChampionList.Data[championName].Lore.Replace("<br>", "\n"));
            }
            else
            {
                await Context.Channel.SendMessageAsync("Nieprawidłowa nazwa bohatera");
            }
        }

        [Command("showSkills", RunMode = RunMode.Async)]
        [Summary("Wyświetla lore podanego bohatera z League of Legends")]
        public async Task ShowSkills(string championName = null)
        {
            string apiKey = m_Map.GetService<APIKeys>().RiotAPIKey;
            await GetChampionList(apiKey);
            //m_ChampionList = await APICalls.GetChampions(null);

            if (m_ChampionList.m_ChampionList.Data.ContainsKey(championName))
            {
                string message = "";
                var champion = m_ChampionList.m_ChampionList.Data[championName];

                Regex rgx = new Regex(@"\{\{ ([eaf])([0-9]) \}\}");
                foreach (var spell in champion.Spells)
                {
                    MatchCollection matches = rgx.Matches(spell.SanitizedTooltip);
                    Dictionary<string, string> keyMatches = new Dictionary<string, string>();
                    foreach (Match key in matches)
                    {
                        if (!keyMatches.ContainsKey(key.Groups[0].Value))
                        {
                            switch (key.Groups[1].Value)
                            {
                                case "f":
                                    {
                                        var tmp = spell.Vars.Where(x => x.Key == $"f{key.Groups[2].Value}");
                                        if (tmp.Any())
                                            keyMatches.Add(key.Groups[0].Value, tmp.First().Coeff.First().ToString() + "x " + tmp.First().Link);
                                        else
                                        {
                                            keyMatches.Add($" (+{key.Groups[0].Value})", "");
                                        }
                                        break;
                                    }
                                case "a":
                                    {
                                        var tmp = spell.Vars.Where(x => x.Key == $"a{key.Groups[2].Value}");
                                        if (tmp.Any())
                                            keyMatches.Add(key.Groups[0].Value, tmp.First().Coeff.First().ToString() + "x " + tmp.First().Link);
                                        else
                                            keyMatches.Add($" (+{key.Groups[0].Value})", "");
                                        break;
                                    }
                                case "e":
                                    var values = spell.EffectBurn[int.Parse(key.Groups[2].Value)];
                                    keyMatches.Add(key.Groups[0].Value, values);
                                    break;
                            }
                        }
                    }
                    string skillMessage = $"```{spell.Name} - {spell.SanitizedTooltip}```";
                    foreach (var key in keyMatches)
                    {
                        skillMessage = skillMessage.Replace(key.Key, key.Value);
                    }
                    await Context.Channel.SendMessageAsync(skillMessage);
                }

                //await Context.Channel.SendMessageAsync(message);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Nieprawidłowa nazwa bohatera");
            }
        }

        [Command("showEnemyLoL", RunMode = RunMode.Async)]
        [Summary("Wyświetla rangi aktualnych przeciwników w grze League of Legends")]
        public async Task ShowEnemyLoL(string summonerName = null)
        {
            string apiKey = m_Map.GetService<APIKeys>().RiotAPIKey;
            await GetChampionList(apiKey);
            //m_ChampionList = await APICalls.GetChampions(null);

            if (summonerName == null)
                summonerName = m_SummonerNames[Context.User.Id];
            else
                summonerName = summonerName.Replace("\"", "").Replace(" ", "");

            var summonerObj = await APICalls.GetSummoner(Context.Channel, apiKey, summonerName);
            if (summonerObj == null)
                return;
            //long summonerId = long.Parse(summonerObj.First.First["id"].ToString());
            long summonerId = summonerObj[summonerName.ToLower()].ID;

            var curGameObj = await APICalls.GetCurrentGame(summonerId, Context.Channel, apiKey);
            if (curGameObj == null)
                return;

            List<string> summonerIDs = new List<string>();
            Dictionary<long, string> inMatchIDs = new Dictionary<long, string>();
            List<long> blueTeamIds = curGameObj.Participants.Where(x => x.TeamID == 100).Select(x => x.SummonerID).ToList();
            List<long> redTeamIds = curGameObj.Participants.Where(x => x.TeamID == 200).Select(x => x.SummonerID).ToList();
            foreach (var participant in curGameObj.Participants)
            {
                summonerIDs.Add($"{participant.SummonerID}");
                inMatchIDs[participant.SummonerID] = participant.SummonerName;
            }

            var leaguesObj = await APICalls.GetLeagues(apiKey, summonerIDs.ToArray());
            List<int> teams = new List<int>();
            teams.Add(100);
            teams.Add(200);

            string message = "Twoja aktualna gra:\n";
            foreach(var team in teams)
            {
                if(team == 100)
                    message += "```md\n#Blue team:\n";
                else
                    message += "```diff\n-Red team:\n";

                foreach (var teamPlayer in curGameObj.Participants.Where(x => x.TeamID == team))
                {
                    if (leaguesObj.ContainsKey(teamPlayer.SummonerID.ToString()))
                    {
                        bool foundSolo = false;
                        foreach (var league in leaguesObj[teamPlayer.SummonerID.ToString()])
                        {
                            if (league.Queue == "RANKED_SOLO_5x5")
                            {
                                Champion champion = m_ChampionList.m_ChampionList.Data.Where(x => x.Value.ID == teamPlayer.ChampionID).First().Value;
                                message += $"{league.Tier,7} {league.Entries.First().Division,3} {"("+champion.Name+")",11} - {league.Entries.First().PlayerOrTeamName}\n";
                                foundSolo = true;
                            }
                        }
                        if (!foundSolo)
                        {
                            Champion champion = m_ChampionList.m_ChampionList.Data.Where(x => x.Value.ID == teamPlayer.ChampionID).First().Value;
                            message += $"Unranked    {"("+champion.Name+")",11} - {teamPlayer.SummonerName}\n";
                        }
                    }
                    else
                    {
                        Champion champion = m_ChampionList.m_ChampionList.Data.Where(x => x.Value.ID == teamPlayer.ChampionID).First().Value;
                        message += $"Unranked    {"("+champion.Name+")",11} - {teamPlayer.SummonerName}\n";
                    }
                }

                message += "```";
            }

            await Context.Channel.SendMessageAsync(message);
            
        }

        public static void Install(IServiceProvider map)
        {
            var client = map.GetService<DiscordSocketClient>();

            client.GuildMemberUpdated += GameChanged;
        }

        private static Task GameChanged(SocketGuildUser before, SocketGuildUser after)
        {
            throw new NotImplementedException();
        }

        private class APICalls
        {
            public static async Task<Dictionary<string, List<League>>> GetLeagues(string apiKey, params string[] summonerIDs)
            {
                var webClient = new HttpClient();

                string idString = "";
                foreach (var summonerID in summonerIDs)
                    idString += summonerID + ",";
                idString = idString.Substring(0, idString.Length - 1);

                HttpResponseMessage leagues = await webClient.GetAsync($"https://eune.api.pvp.net/api/lol/EUNE/v2.5/league/by-summoner/{idString}/entry?api_key={apiKey}");
                var reader = new StreamReader(await leagues.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<Dictionary<string, List<League>>>(await reader.ReadToEndAsync());
            }

            public static async Task<CurrentGameInfo> GetCurrentGame(long summonerId, IMessageChannel log, string apiKey)
            {
                var webClient = new HttpClient();
                HttpResponseMessage game = await webClient.GetAsync($"https://eune.api.riotgames.com/observer-mode/rest/consumer/getSpectatorGameInfo/EUN1/{summonerId}?api_key={apiKey}");
                if (game.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await log.SendMessageAsync("Nie grasz aktualnie w żadnym meczu");
                    return null;
                }
                var reader = new StreamReader(await game.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<CurrentGameInfo>(await reader.ReadToEndAsync());
            }

            public static async Task<Dictionary<string, Summoner>> GetSummoner(IMessageChannel log, string apiKey, params string[] summonerNames)
            {
                var webClient = new HttpClient();

                string nameString = "";
                foreach (var summonerName in summonerNames)
                    nameString += summonerName + ",";
                nameString = nameString.Substring(0, nameString.Length - 1);

                HttpResponseMessage summoner = await webClient.GetAsync($"https://eune.api.pvp.net/api/lol/eune/v1.4/summoner/by-name/{nameString}?api_key={apiKey}");
                if (summoner.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await log.SendMessageAsync("Nie znam twojej nazwy przywoływacza lub jest ona nieprawidłowa");
                    return null;
                }
                var reader = new StreamReader(await summoner.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<Dictionary<string,Summoner>>(await reader.ReadToEndAsync());
            }

            public static async Task<ChampionList> GetChampions(IMessageChannel log, string apiKey)
            {
                var webClient = new HttpClient();

                HttpResponseMessage summoner = await webClient.GetAsync($"https://eun1.api.riotgames.com/lol/static-data/v3/champions?champData=all&locale=pl_PL&api_key={apiKey}");
                if (summoner.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await log.SendMessageAsync("Błąd przy pobieraniu championów z serwera API Riot");
                    return null;
                }
                var reader = new StreamReader(await summoner.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<ChampionList>(await reader.ReadToEndAsync());
            }
        }

    }
}
