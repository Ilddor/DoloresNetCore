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

namespace Dolores.Modules.Games
{
    public class RiotAPI : ModuleBase
    {
        IDependencyMap m_Map;
        Dictionary<ulong, string> m_SummonerNames;

        public RiotAPI(IDependencyMap map)
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
        }

        [Command("showEnemyLoL")]
        [Summary("Wyświetla rangi aktualnych przeciwników w grze League of Legends")]
        public async Task ShowEnemyLoL(string summonerName = null)
        {
            if (summonerName == null)
                summonerName = m_SummonerNames[Context.User.Id];
            else
                summonerName = summonerName.Replace("\"", "").Replace(" ", "");

            var summonerObj = await APICalls.GetSummoner(Context.Channel, summonerName);
            if (summonerObj == null)
                return;
            //long summonerId = long.Parse(summonerObj.First.First["id"].ToString());
            long summonerId = summonerObj[summonerName.ToLower()].ID;

            var curGameObj = await APICalls.GetCurrentGame(summonerId, Context.Channel);
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

            var leaguesObj = await APICalls.GetLeagues(summonerIDs.ToArray());

            string message = "Twoja aktualna gra:\n";
            message += "```md\n#Blue team:\n";
            foreach(var teamPlayer in blueTeamIds)
            {
                if(leaguesObj.ContainsKey(teamPlayer.ToString()))
                {
                    bool foundSolo = false;
                    foreach (var league in leaguesObj[teamPlayer.ToString()])
                    {
                        if (league.Queue == "RANKED_SOLO_5x5")
                        {
                            message += $"{league.Entries.First().PlayerOrTeamName,15} - {league.Tier} {league.Entries.First().Division}\n";
                            foundSolo = true;
                        }
                    }
                    if(!foundSolo)
                    {
                        message += $"{inMatchIDs[teamPlayer],15} - Unranked\n";
                    }
                }
                else
                {
                    message += $"{inMatchIDs[teamPlayer],15} - Unranked\n";
                }
            }
            message += "```";

            message += "```diff\n-Red team:\n";
            foreach (var teamPlayer in redTeamIds)
            {
                if (leaguesObj.ContainsKey(teamPlayer.ToString()))
                {
                    bool foundSolo = false;
                    foreach (var league in leaguesObj[teamPlayer.ToString()])
                    {
                        if (league.Queue == "RANKED_SOLO_5x5")
                        {
                            message += $"{league.Entries.First().PlayerOrTeamName,15} - {league.Tier} {league.Entries.First().Division}\n";
                            foundSolo = true;
                        }
                    }
                    if (!foundSolo)
                    {
                        message += $"{inMatchIDs[teamPlayer],15} - Unranked\n";
                    }
                }
                else
                {
                    message += $"{inMatchIDs[teamPlayer],15} - Unranked\n";
                }
            }
            message += "```\n";

            await Context.Channel.SendMessageAsync(message);
            
        }

        public static void Install(IDependencyMap map)
        {
            var client = map.Get<DiscordSocketClient>();

            client.GuildMemberUpdated += GameChanged;
        }

        private static Task GameChanged(SocketGuildUser before, SocketGuildUser after)
        {
            throw new NotImplementedException();
        }

        private class APICalls
        {
            public static async Task<Dictionary<string, List<League>>> GetLeagues(params string[] summonerIDs)
            {
                var webClient = new HttpClient();

                string idString = "";
                foreach (var summonerID in summonerIDs)
                    idString += summonerID + ",";
                idString = idString.Substring(0, idString.Length - 1);

                HttpResponseMessage leagues = await webClient.GetAsync($"https://eune.api.pvp.net/api/lol/EUNE/v2.5/league/by-summoner/{idString}/entry?api_key={Dolores.m_Instance.m_APIKeys.RiotAPIKey}");
                var reader = new StreamReader(await leagues.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<Dictionary<string, List<League>>>(await reader.ReadToEndAsync());
            }

            public static async Task<CurrentGameInfo> GetCurrentGame(long summonerId, IMessageChannel log)
            {
                var webClient = new HttpClient();
                HttpResponseMessage game = await webClient.GetAsync($"https://eune.api.riotgames.com/observer-mode/rest/consumer/getSpectatorGameInfo/EUN1/{summonerId}?api_key={Dolores.m_Instance.m_APIKeys.RiotAPIKey}");
                if (game.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await log.SendMessageAsync("Nie grasz aktualnie w żadnym meczu");
                    return null;
                }
                var reader = new StreamReader(await game.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<CurrentGameInfo>(await reader.ReadToEndAsync());
            }

            public static async Task<Dictionary<string, Summoner>> GetSummoner(IMessageChannel log, params string[] summonerNames)
            {
                var webClient = new HttpClient();

                string nameString = "";
                foreach (var summonerName in summonerNames)
                    nameString += summonerName + ",";
                nameString = nameString.Substring(0, nameString.Length - 1);

                HttpResponseMessage summoner = await webClient.GetAsync($"https://eune.api.pvp.net/api/lol/eune/v1.4/summoner/by-name/{nameString}?api_key={Dolores.m_Instance.m_APIKeys.RiotAPIKey}");
                if (summoner.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await log.SendMessageAsync("Nie znam twojej nazwy przywoływacza lub jest ona nieprawidłowa");
                    return null;
                }
                var reader = new StreamReader(await summoner.Content.ReadAsStreamAsync());
                return JsonConvert.DeserializeObject<Dictionary<string,Summoner>>(await reader.ReadToEndAsync());
            }
        }

    }
}
