﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Drawing.Imaging;

namespace Dolores.Modules.Games
{
    public class GameTime : ModuleBase
    {
        IServiceProvider m_Map;

        public GameTime(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("showGameTime", RunMode = RunMode.Async)]
        [Summary("Pokazuje czas gry spędzony w konkretnych grach")]
        public async Task ShowGameTime()
        {
            // TODO: refactor this!

            var gameTimes = m_Map.GetService<GameTimes>();
            var client = m_Map.GetService<DiscordSocketClient>();
            int lineCount = 1;
            Bitmap image = null;
            gameTimes.m_Mutex.WaitOne();
            try
            {
                foreach (var user in gameTimes.m_Times)
                {
                    if (client.GetUser(user.Key) == null)
                        continue; // skip non existent user on this guild
                    lineCount++;
                    foreach (var game in user.Value)
                    {
                        lineCount++;
                    }
                }

                // TODO: think of copying DB outside mutex scope to avoid long locks
                int startX = 10, startY = 10;
                Font drawFont = new Font(SystemFonts.DefaultFont.FontFamily, 18, FontStyle.Regular, GraphicsUnit.Point);
                image = new Bitmap(640, (int)((drawFont.Height) * lineCount) + 20);

                image.SetResolution(96, 96);
                var graphics = Graphics.FromImage(image);
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.CompositingMode = CompositingMode.SourceOver;

                var regionAll = new Region(new Rectangle(0, 0, image.Width, image.Height));
                graphics.FillRegion(Brushes.Black, regionAll);

                
                var fontSize = graphics.MeasureString("Test", drawFont);

                float posX = startX;
                float posY = startY;

                graphics.DrawString("Czas gry:", drawFont, Brushes.White, posX, posY);
                posY += drawFont.Size + 10;

                foreach (var user in gameTimes.m_Times)
                {
                    if (client.GetUser(user.Key) == null)
                        continue; // skip non existent user on this guild
                    var userName = client.GetUser(user.Key).Username;

                    string line = $"{userName}: ";
                    graphics.DrawString(line, drawFont, Brushes.White, posX, posY);
                    posY += drawFont.Size + 10;

                    foreach (var game in user.Value)
                    {
                        line = $"    - {game.Key}: ";
                        TimeSpan time = new TimeSpan(game.Value);
                        if (time.Days != 0)
                            line += $"{time.Days}d ";
                        line += $"{time.ToString(@"hh\:mm\:ss")}\n";

                        graphics.DrawString(line, drawFont, Brushes.White, posX, posY);
                        posY += drawFont.Size + 10;
                    }
                }

                graphics.Save();
                drawFont.Dispose();
            }
            catch (Exception) { }
            gameTimes.m_Mutex.ReleaseMutex();

            var fileOutput = File.Open($"GameTime.png", FileMode.OpenOrCreate);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
            var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
            image.Save(fileOutput, codec, encoderParameters);
            fileOutput.Close();

            //Context.Message.DeleteAsync();
            await Context.Channel.SendFileAsync($"GameTime.png");
        }

        public static void Install(IServiceProvider map)
        {
            var client = map.GetService<DiscordSocketClient>();
            client.GuildMemberUpdated += GameChanged;
        }

        private static Task GameChanged(SocketGuildUser before, SocketGuildUser after)
        {
            var gameTimes = Dolores.m_Instance.map.GetService<GameTimes>();
            if(after.Guild.Id == 269960016591716362)
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
