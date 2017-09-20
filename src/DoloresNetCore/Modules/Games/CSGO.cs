using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net;

namespace Dolores.Modules.Games
{
    public class CSGO : ModuleBase
    {
        IServiceProvider m_Map;
        public CSGO(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("CSGO", RunMode = RunMode.Async)]
        [Summary("Wyświetla staty danego gracza w CS:GO (trzeba podac login steam zamiast nick)")]
        public async Task CSGOStats(string name)
        {
            string steamWebAPIKey = (Dolores.m_Instance.m_APIKeys.SteamWebAPIKey);

            var webClient = new HttpClient();
            HttpResponseMessage steamIDResponse = await webClient.GetAsync($"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={steamWebAPIKey}&vanityurl={name}");
            var reader = new StreamReader(await steamIDResponse.Content.ReadAsStreamAsync());
            var response = JsonConvert.DeserializeObject<Dictionary<string,Steam.DataObjects.SteamUserIDResponse>>(await reader.ReadToEndAsync());

            HttpResponseMessage gameStatsResponse = await webClient.GetAsync($"http://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v0002/?appid=730&key={steamWebAPIKey}&steamid={response["response"].SteamID}");
            reader = new StreamReader(await gameStatsResponse.Content.ReadAsStreamAsync());
            var statsResponse = JsonConvert.DeserializeObject<Dictionary<string, Steam.DataObjects.PlayerStats>>(await reader.ReadToEndAsync());

            var image = RenderStats(statsResponse["playerstats"], name);

            var fileOutput = File.Open($"StatsCSGO/{name}.png", FileMode.OpenOrCreate);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
            var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
            image.Save(fileOutput, codec, encoderParameters);
            fileOutput.Close();

            Context.Message.DeleteAsync();
            var channelCSGOStats = await Context.Guild.GetTextChannelAsync(360033939257032704);
            await channelCSGOStats.SendFileAsync($"StatsCSGO/{name}.png");
        }

        private Bitmap RenderStats(Steam.DataObjects.PlayerStats stats, string name)
        {
            int startX = 10, startY = 10;
            var image = new Bitmap(640, 480);

            image.SetResolution(96, 96);
            var graphics = Graphics.FromImage(image);
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.CompositingMode = CompositingMode.SourceOver;

            // Fill red to easily spot errors
            var regionAll = new Region(new Rectangle(0, 0, image.Width, image.Height));
            graphics.FillRegion(Brushes.Red, regionAll);

            // Fill background
            var background = new Bitmap(System.Drawing.Image.FromFile("CSGOBackground.jpg"));
            graphics.DrawImage(background, 0, 0, image.Width, image.Height);

            // And shade it a little bit
            var shadingBrush = new SolidBrush(System.Drawing.Color.FromArgb(190, 50, 50, 50));
            graphics.FillRegion(shadingBrush, regionAll);

            var toRender = new Tuple<string, string>[4, 3];

            toRender[0, 0] = new Tuple<string, string>("Kills: ", stats.Stats.Find(x => x.Name == "total_kills").Value.ToString());
            toRender[0, 1] = new Tuple<string, string>("Headshots: ", stats.Stats.Find(x => x.Name == "total_kills_headshot").Value.ToString());
            toRender[0, 2] = new Tuple<string, string>("Knife kills: ", stats.Stats.Find(x => x.Name == "total_kills_knife").Value.ToString());
            toRender[1, 0] = new Tuple<string, string>("DMG: ", stats.Stats.Find(x => x.Name == "total_damage_done").Value.ToString());
            toRender[1, 1] = new Tuple<string, string>("Money: ", stats.Stats.Find(x => x.Name == "total_money_earned").Value.ToString());
            toRender[1, 2] = new Tuple<string, string>("MVP: ", stats.Stats.Find(x => x.Name == "total_mvps").Value.ToString());
            toRender[2, 0] = new Tuple<string, string>("Taser kills: ", stats.Stats.Find(x => x.Name == "total_kills_taser").Value.ToString());
            toRender[2, 1] = new Tuple<string, string>("Bombs planted: ", stats.Stats.Find(x => x.Name == "total_planted_bombs").Value.ToString());
            var survived = float.Parse(stats.Stats.Find(x => x.Name == "total_time_played").Value.ToString());
            toRender[2, 2] = new Tuple<string, string>("Played: ", $"{(int)(survived / 3600)}h {(int)((survived % 3600) / 60)}m {(int)(survived % 60)}s");
            toRender[3, 0] = null;
            toRender[3, 1] = new Tuple<string, string>("Wins: ", stats.Stats.Find(x => x.Name == "total_wins").Value.ToString());
            toRender[3, 2] = null;

            Font drawFont = new Font(SystemFonts.DefaultFont.FontFamily, 18, FontStyle.Regular, GraphicsUnit.Point);
            String playerName = name;
            var nameSize = graphics.MeasureString(playerName, drawFont);
            graphics.DrawString(playerName, drawFont, Brushes.White, image.Width / 2 - nameSize.Width / 2, startY);

            float posX = startX;
            float posY = startY + nameSize.Height;

            posY += 30;

            SizeF statsChunk = new SizeF((image.Width - 20) / toRender.GetLength(1), (image.Height - posY - 10) / toRender.GetLength(0));
            for (int row = 0; row < toRender.GetLength(0); row++)
            {
                for (int col = 0; col < toRender.GetLength(1); col++)
                {
                    if (toRender[row, col] != null)
                    {
                        // Line
                        graphics.DrawLine(
                            new Pen(Brushes.DimGray),
                            new PointF(startX + col * statsChunk.Width + 10, posY + row * statsChunk.Height + statsChunk.Height / 2),
                            new PointF(startX + (col + 1) * statsChunk.Width - 10, posY + row * statsChunk.Height + statsChunk.Height / 2));
                        PointF chunkCorner = new PointF(startX + col * statsChunk.Width, posY + row * statsChunk.Height);
                        var categorySize = graphics.MeasureString(toRender[row, col].Item1, drawFont);
                        var valueSize = graphics.MeasureString(toRender[row, col].Item2, drawFont);

                        // Category
                        graphics.DrawString(
                            toRender[row, col].Item1,
                            drawFont,
                            Brushes.DarkGray,
                            chunkCorner.X + statsChunk.Width / 2 - categorySize.Width / 2,
                            chunkCorner.Y + statsChunk.Height / 2 - categorySize.Height);

                        // Value
                        graphics.DrawString(
                            toRender[row, col].Item2,
                            drawFont,
                            Brushes.White,
                            chunkCorner.X + statsChunk.Width / 2 - valueSize.Width / 2,
                            chunkCorner.Y + statsChunk.Height / 2);
                    }
                }
            }

            graphics.Save();
            drawFont.Dispose();
            //fonts.Dispose();
            return image;
        }
    }
}
