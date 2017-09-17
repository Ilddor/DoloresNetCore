using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores;
using PUBGSharp;
using PUBGSharp.Exceptions;
using PUBGSharp.Helpers;
using System.IO;
using System.Drawing.Imaging;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Net;
using System.Drawing.Text;

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
            int startX = 10, startY = 10;
            using (var image = new Bitmap(640,480))
            {
                var graphics = Graphics.FromImage(image);
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.CompositingMode = CompositingMode.SourceOver;

                // Fill red to easily spot errors
                var regionAll = new Region(new Rectangle(0, 0, image.Width, image.Height));
                graphics.FillRegion(Brushes.Red, regionAll);

                // Fill background
                var background = new Bitmap(System.Drawing.Image.FromFile("PUBG.png"));
                graphics.DrawImage(background, 0, 0, image.Width, image.Height);

                // And shade it a little bit
                var shadingBrush = new SolidBrush(System.Drawing.Color.FromArgb(190, 50, 50, 50));
                graphics.FillRegion(shadingBrush, regionAll);

                // Draw avatar
                WebRequest avatarRequest = WebRequest.Create(stats.Avatar);
                var avatar = new Bitmap(System.Drawing.Image.FromStream(avatarRequest.GetResponse().GetResponseStream()));
                graphics.DrawImage(avatar, startX, startY, avatar.Width, avatar.Height);
                // Yes I do know GetResponse() can fail etc. I just assume it'll be fine, TODO: add error handling

                var lastMatch = stats.MatchHistory[0];

                // Draw player name
                PrivateFontCollection fonts = new PrivateFontCollection();
                fonts.AddFontFile("Teko-Regular.ttf");
                Font drawFont = new Font(fonts.Families[0], 20, FontStyle.Regular, GraphicsUnit.Point);
                String playerName = stats.PlayerName;
                var nameSize = graphics.MeasureString(playerName, drawFont);
                graphics.DrawString(playerName, drawFont, Brushes.White, avatar.Width + startX, startY);

                float posX = startX;
                float posY = startY + avatar.Height;

                // Rating with change
                string playerRankPart1 = $"Rank: {lastMatch.RatingRank} (";
                string playerRankPart2 = $"{lastMatch.RatingRankChange}";
                string playerRankPart3 = $")";
                var part1Size = graphics.MeasureString(playerRankPart1, drawFont);
                var part2Size = graphics.MeasureString(playerRankPart2, drawFont);
                
                graphics.DrawString(playerRankPart1, drawFont, Brushes.White, posX, posY);
                Brush changeBrush = lastMatch.RatingRankChange >= 0 ? Brushes.Green : Brushes.Red;
                posX += part1Size.Width;
                graphics.DrawString(playerRankPart2, drawFont, changeBrush, posX, posY);
                posX += part2Size.Width;
                graphics.DrawString(playerRankPart3, drawFont, Brushes.White, posX, posY);

                string typeString = "Last Day Played";
                var typeSize = graphics.MeasureString(typeString, drawFont);
                graphics.DrawString(typeString, drawFont, Brushes.White, image.Width / 2 - typeSize.Width / 2, startY);

                string dateString = lastMatch.Updated.ToString("yyyy-MM-dd");
                var dateSize = graphics.MeasureString(dateString, drawFont);
                graphics.DrawString(dateString, drawFont, Brushes.White, image.Width - dateSize.Width - 10 , startY);

                posY += 30;
                /*graphics.DrawString($"Assists: {lastMatch.Assists}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"DMG: {lastMatch.Damage}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Kills: {lastMatch.Kills}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Headshots: {lastMatch.Headshots}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"K/D: {lastMatch.Kd}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Traveled: {lastMatch.MoveDistance}m", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Rounds played: {lastMatch.Rounds}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Top 10s: {lastMatch.Top10}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Survived: {lastMatch.TimeSurvived}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Wins: {lastMatch.Wins}", drawFont, Brushes.White, startX, posY);*/

                Tuple<string, string>[,] statsToRender = new Tuple<string, string>[4, 3];
                statsToRender[0, 0] = new Tuple<string, string>("Kills: ", lastMatch.Kills.ToString());
                statsToRender[0, 1] = new Tuple<string, string>("Headshots: ", lastMatch.Headshots.ToString());
                statsToRender[0, 2] = new Tuple<string, string>("Assists: ", lastMatch.Assists.ToString());
                statsToRender[1, 0] = new Tuple<string, string>("DMG: ", lastMatch.Damage.ToString());
                statsToRender[1, 1] = new Tuple<string, string>("K/D: ", lastMatch.Kd.ToString());
                statsToRender[1, 2] = new Tuple<string, string>("Traveled: ", lastMatch.MoveDistance.ToString());
                statsToRender[2, 0] = new Tuple<string, string>("Rounds played: ", lastMatch.Rounds.ToString());
                statsToRender[2, 1] = new Tuple<string, string>("Top 10s: ", lastMatch.Top10.ToString());
                statsToRender[2, 2] = new Tuple<string, string>("Survived: ", $"{(int)(lastMatch.TimeSurvived / 60)}m {(int)(lastMatch.TimeSurvived % 60)}s");
                statsToRender[3, 0] = null;
                statsToRender[3, 1] = new Tuple<string, string>("Wins: ", lastMatch.Wins.ToString());
                statsToRender[3, 2] = null;

                SizeF statsChunk = new SizeF((image.Width - 20) / statsToRender.GetLength(1), (image.Height - posY - 10) / statsToRender.GetLength(0));
                for(int row = 0; row < statsToRender.GetLength(0); row++)
                {
                    for (int col = 0; col < statsToRender.GetLength(1); col++)
                    {
                        if (statsToRender[row, col] != null)
                        {
                            // Line
                            graphics.DrawLine(
                                new Pen(Brushes.DimGray),
                                new PointF(startX + col * statsChunk.Width + 10, posY + row * statsChunk.Height + statsChunk.Height / 2),
                                new PointF(startX + (col + 1) * statsChunk.Width - 10, posY + row * statsChunk.Height + statsChunk.Height / 2));
                            PointF chunkCorner = new PointF(startX + col * statsChunk.Width, posY + row * statsChunk.Height);
                            var categorySize = graphics.MeasureString(statsToRender[row, col].Item1, drawFont);
                            var valueSize = graphics.MeasureString(statsToRender[row, col].Item2, drawFont);

                            // Category
                            graphics.DrawString(
                                statsToRender[row, col].Item1,
                                drawFont,
                                Brushes.DarkGray,
                                chunkCorner.X + statsChunk.Width / 2 - categorySize.Width / 2,
                                chunkCorner.Y + statsChunk.Height / 2 - categorySize.Height);

                            // Value
                            graphics.DrawString(
                                statsToRender[row, col].Item2,
                                drawFont,
                                Brushes.White,
                                chunkCorner.X + statsChunk.Width / 2 - valueSize.Width / 2,
                                chunkCorner.Y + statsChunk.Height / 2);
                        }
                    }
                }

                graphics.Save();
                var fileOutput = File.Open($"PUBGStats/{name}.png", FileMode.OpenOrCreate);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
                var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
                image.Save(fileOutput, codec, encoderParameters);
                fileOutput.Close();
                drawFont.Dispose();
                fonts.Dispose();
            }
            await Context.Channel.SendFileAsync($"PUBGStats/{name}.png");
        }
    }
}
