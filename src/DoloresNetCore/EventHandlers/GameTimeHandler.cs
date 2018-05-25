using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dolores.DataClasses;
using System.Linq;

namespace Dolores.EventHandlers
{
    class GameTimeHandler : IInstallable
    {
        private IServiceProvider m_Map;

        public Task Install(IServiceProvider map)
        {
            m_Map = map;
            var client = m_Map.GetService<DiscordSocketClient>();
            client.GuildMemberUpdated += GameChanged;

            return Task.CompletedTask;
        }

        private async Task GameChanged(SocketGuildUser before, SocketGuildUser after)
        {
            var gameTimes = m_Map.GetService<GameTimes>();
            if(after.IsBot)
            {
                gameTimes.m_Mutex.WaitOne();
                try
                {
                    gameTimes.m_Times.Remove(after.Id);
                }
                catch (Exception) { }
                gameTimes.m_Mutex.ReleaseMutex();
                return;
            }
            //if (after.Guild.Id == 269960016591716362)
            {
                if (before.Activity.Name.Any() || !after.Activity.Name.Any())
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

                if (after.Activity.Name.Any())
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

                        gameTimes.m_StartTimes[after.Id] = new Tuple<string, DateTime>(after.Activity.Name, DateTime.Now);
                    }
                    catch (Exception) { }
                    gameTimes.m_Mutex.ReleaseMutex();
                }
            }

            return;
        }
    }
}
