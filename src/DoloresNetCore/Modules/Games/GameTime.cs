using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;

namespace Dolores.Modules.Games
{
    public class GameTime : ModuleBase
    {
        IDependencyMap m_Map;

        public GameTime(IDependencyMap map)
        {
            m_Map = map;
        }

        [Command("showGameTime", RunMode = RunMode.Async)]
        [Summary("Pokazuje czas gry spędzony w konkretnych grach")]
        public async Task ShowGameTime()
        {
            var gameTimes = m_Map.Get<GameTimes>();
            var client = m_Map.Get<DiscordSocketClient>();
            string message = "Czas gry\n```";
            gameTimes.m_Mutex.WaitOne();
            try
            {
                foreach (var user in gameTimes.m_Times)
                {
                    var userName = client.GetUser(user.Key).Username;
                    message += $"{userName}: \n";
                    foreach (var game in user.Value)
                    {
                        message += $"    - {game.Key}: ";
                        TimeSpan time = new TimeSpan(game.Value);
                        if (time.Days != 0)
                            message += $"{time.Days}d ";
                        message += $"{time.ToString(@"hh\:mm\:ss")}\n";
                    }
                }
            }
            catch (Exception) { }
            gameTimes.m_Mutex.ReleaseMutex();
            message += "```";
            await Context.Channel.SendMessageAsync(message);
        }

        public static void Install(IDependencyMap map)
        {
            var client = map.Get<DiscordSocketClient>();
            client.GuildMemberUpdated += GameChanged;
        }

        private static Task GameChanged(SocketGuildUser before, SocketGuildUser after)
        {
            var gameTimes = Dolores.m_Instance.map.Get<GameTimes>();
            if(after.Guild.Name == "SurowcowaPL")
            {
                if(before.Game.HasValue || !after.Game.HasValue)
                {
                    gameTimes.m_Mutex.WaitOne();
                    try
                    {
                        if (gameTimes.m_StartTimes.ContainsKey(after.Id))
                        {
                            var timeSpent = DateTime.Now - gameTimes.m_StartTimes[after.Id].Item2;
                            if (!gameTimes.m_Times.ContainsKey(after.Id))
                                gameTimes.m_Times[after.Id] = new Dictionary<string, long>();

                            if (!gameTimes.m_Times[after.Id].ContainsKey(gameTimes.m_StartTimes[after.Id].Item1))
                                gameTimes.m_Times[after.Id][gameTimes.m_StartTimes[after.Id].Item1] = timeSpent.Ticks;
                            else
                                gameTimes.m_Times[after.Id][gameTimes.m_StartTimes[after.Id].Item1] += timeSpent.Ticks;

                            gameTimes.m_StartTimes.Remove(after.Id);
                        }
                    }
                    catch (Exception) { }
                    gameTimes.m_Mutex.ReleaseMutex();
                }

                if(after.Game.HasValue)
                {
                    gameTimes.m_Mutex.WaitOne();
                    try
                    {
                        if (gameTimes.m_StartTimes.ContainsKey(after.Id))
                        {
                            var timeSpent = DateTime.Now - gameTimes.m_StartTimes[after.Id].Item2;
                            if (!gameTimes.m_Times.ContainsKey(after.Id))
                                gameTimes.m_Times[after.Id] = new Dictionary<string, long>();

                            if (!gameTimes.m_Times[after.Id].ContainsKey(gameTimes.m_StartTimes[after.Id].Item1))
                                gameTimes.m_Times[after.Id][gameTimes.m_StartTimes[after.Id].Item1] = timeSpent.Ticks;
                            else
                                gameTimes.m_Times[after.Id][gameTimes.m_StartTimes[after.Id].Item1] += timeSpent.Ticks;

                            gameTimes.m_StartTimes.Remove(after.Id);
                        }

                        gameTimes.m_StartTimes[after.Id] = new Tuple<string, DateTime>(after.Game.Value.Name, DateTime.Now);
                    }
                    catch (Exception) { }
                    gameTimes.m_Mutex.ReleaseMutex();
                }
            }

            return Task.CompletedTask;
        }
    }
}
