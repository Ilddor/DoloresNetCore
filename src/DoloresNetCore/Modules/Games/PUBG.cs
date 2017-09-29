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
using System.Net.Http;
using Newtonsoft.Json;
using PUBGSharp.Net.Model;

namespace Dolores.Modules.Games
{
    public class PUBG : ModuleBase
    {
        IServiceProvider m_Map;
        public PUBG(IServiceProvider map)
        {
            m_Map = map;
        }

        public enum StatType
        {
            Overall,
            Top,
            Last,
        }

        public class InfoToRender
        {
            public Tuple<string, string>[,] m_Stats;
            public int m_Rank = 0;
            public int m_RankChange = 0;
            public string m_Title;
            public string m_Date;
        }

        [Command("PUBG", RunMode = RunMode.Async)]
        [Summary("Wyświetla staty danego gracza w PUBG")]
        public async Task PUBGStats(string name, PUBGSharp.Data.Mode mode, StatType type)
        {
            var statsClient = new PUBGStatsClient(Dolores.m_Instance.m_APIKeys.PUBGTrackerKey);
            PUBGSharp.Net.Model.StatsResponse stats = await statsClient.GetPlayerStatsAsync(name);

            var image = RenderStats(stats, mode, type);

            var fileOutput = File.Open($"StatsPUBG/{name}.png", FileMode.OpenOrCreate);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
            var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
            image.Save(fileOutput, codec, encoderParameters);
            fileOutput.Close();

            Context.Message.DeleteAsync();
            var channelPUBGStats = await Context.Guild.GetTextChannelAsync(359789815576788992);
            await channelPUBGStats.SendFileAsync($"StatsPUBG/{name}.png");
        }

        private Bitmap RenderStats(PUBGSharp.Net.Model.StatsResponse stats, PUBGSharp.Data.Mode mode, StatType type)
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

            InfoToRender statsToRender = null;
            switch (type)
            {
                case StatType.Overall:
                    statsToRender = FillStatArrayOverall(stats, mode);
                    break;
                case StatType.Top:
                    statsToRender = FillStatArrayTop(stats, mode);
                    break;
                case StatType.Last:
                    statsToRender = FillStatArrayLast(stats, mode);
                    break;
            }

            //var lastMatch = stats.MatchHistory[0];

            // Draw player name
            //PrivateFontCollection fonts = new PrivateFontCollection();
            //fonts.AddFontFile("Teko-Regular.ttf");
            //Font drawFont = new Font(fonts.Families[0], 20, FontStyle.Regular, GraphicsUnit.Point);
            // Looks like bug in CoreCompat? Font does not get to DrawString on Linux(Debian) - wait for .net standard 2.1?
            Font drawFont = new Font(SystemFonts.DefaultFont.FontFamily, 18, FontStyle.Regular, GraphicsUnit.Point);
            String playerName = stats.PlayerName;
            var nameSize = graphics.MeasureString(playerName, drawFont);
            graphics.DrawString(playerName, drawFont, Brushes.White, avatar.Width + startX, startY);

            float posX = startX;
            float posY = startY + avatar.Height;

            // Rating with change
            string playerRankPart1 = $"Rank: {statsToRender.m_Rank}";
            if(statsToRender.m_RankChange != 0)
                playerRankPart1 += " (";
            string playerRankPart2 = $"{statsToRender.m_RankChange}";
            string playerRankPart3 = $")";
            var part1Size = graphics.MeasureString(playerRankPart1, drawFont);
            var part2Size = graphics.MeasureString(playerRankPart2, drawFont);

            graphics.DrawString(playerRankPart1, drawFont, Brushes.White, posX, posY);
            if (statsToRender.m_RankChange != 0)
            {
                Brush changeBrush = statsToRender.m_RankChange >= 0 ? Brushes.Red : Brushes.Green;
                posX += part1Size.Width;
                graphics.DrawString(playerRankPart2, drawFont, changeBrush, posX, posY);
                posX += part2Size.Width;
                graphics.DrawString(playerRankPart3, drawFont, Brushes.White, posX, posY);
            }

            string typeString = statsToRender.m_Title;
            var typeSize = graphics.MeasureString(typeString, drawFont);
            graphics.DrawString(typeString, drawFont, Brushes.White, image.Width / 2 - typeSize.Width / 2, startY);

            string dateString = statsToRender.m_Date;
            var dateSize = graphics.MeasureString(dateString, drawFont);
            graphics.DrawString(dateString, drawFont, Brushes.White, image.Width - dateSize.Width - 10, startY);

            posY += 30;

            SizeF statsChunk = new SizeF((image.Width - 20) / statsToRender.m_Stats.GetLength(1), (image.Height - posY - 10) / statsToRender.m_Stats.GetLength(0));
            for (int row = 0; row < statsToRender.m_Stats.GetLength(0); row++)
            {
                for (int col = 0; col < statsToRender.m_Stats.GetLength(1); col++)
                {
                    if (statsToRender.m_Stats[row, col] != null)
                    {
                        // Line
                        graphics.DrawLine(
                            new Pen(Brushes.DimGray),
                            new PointF(startX + col * statsChunk.Width + 10, posY + row * statsChunk.Height + statsChunk.Height / 2),
                            new PointF(startX + (col + 1) * statsChunk.Width - 10, posY + row * statsChunk.Height + statsChunk.Height / 2));
                        PointF chunkCorner = new PointF(startX + col * statsChunk.Width, posY + row * statsChunk.Height);
                        var categorySize = graphics.MeasureString(statsToRender.m_Stats[row, col].Item1, drawFont);
                        var valueSize = graphics.MeasureString(statsToRender.m_Stats[row, col].Item2, drawFont);

                        // Category
                        graphics.DrawString(
                            statsToRender.m_Stats[row, col].Item1,
                            drawFont,
                            Brushes.DarkGray,
                            chunkCorner.X + statsChunk.Width / 2 - categorySize.Width / 2,
                            chunkCorner.Y + statsChunk.Height / 2 - categorySize.Height);

                        // Value
                        graphics.DrawString(
                            statsToRender.m_Stats[row, col].Item2,
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

        private InfoToRender FillStatArrayOverall(PUBGSharp.Net.Model.StatsResponse stats, PUBGSharp.Data.Mode mode)
        {
            InfoToRender toRender = new InfoToRender();
            toRender.m_Stats = new Tuple<string, string>[4, 3];

            var modeStats = stats.Stats.Find(x => x.Mode == mode && x.Region == PUBGSharp.Data.Region.AGG);

            toRender.m_Title = $"Overall {mode.ToString()}";
            DateTime date = DateTime.Parse(stats.LastUpdated);
            toRender.m_Date = date.ToString("yyyy-MM-dd");
            toRender.m_Rank = modeStats.Stats.Find(x => x.Stat == Stats.Rating).Rank.Value;

            toRender.m_Stats[0, 0] = new Tuple<string, string>("Kills: ", modeStats.Stats.Find(x => x.Stat == Stats.Kills).Value);
            toRender.m_Stats[0, 1] = new Tuple<string, string>("Headshots: ", modeStats.Stats.Find(x => x.Stat == Stats.HeadshotKills).Value);
            toRender.m_Stats[0, 2] = new Tuple<string, string>("Assists: ", modeStats.Stats.Find(x => x.Stat == Stats.Assists).Value);
            toRender.m_Stats[1, 0] = new Tuple<string, string>("DMG: ", modeStats.Stats.Find(x => x.Stat == Stats.DamageDealt).Value);
            toRender.m_Stats[1, 1] = new Tuple<string, string>("K/D: ", modeStats.Stats.Find(x => x.Stat == Stats.KDR).Value);
            toRender.m_Stats[1, 2] = new Tuple<string, string>("Traveled: ", modeStats.Stats.Find(x => x.Stat == Stats.MoveDistance).Value);
            toRender.m_Stats[2, 0] = new Tuple<string, string>("Rounds played: ", modeStats.Stats.Find(x => x.Stat == Stats.RoundsPlayed).Value);
            toRender.m_Stats[2, 1] = new Tuple<string, string>("Top 10s: ", modeStats.Stats.Find(x => x.Stat == Stats.Top10).Value);
            var survived = float.Parse(modeStats.Stats.Find(x => x.Stat == Stats.TimeSurvived).Value);
            toRender.m_Stats[2, 2] = new Tuple<string, string>("Survived: ", $"{(int)(survived / 3600)}h {(int)((survived % 3600)/60)}m {(int)(survived % 60)}s");
            toRender.m_Stats[3, 0] = null;
            toRender.m_Stats[3, 1] = new Tuple<string, string>("Wins: ", modeStats.Stats.Find(x => x.Stat == Stats.Wins).Value);
            toRender.m_Stats[3, 2] = null;

            return toRender;
        }

        private InfoToRender FillStatArrayTop(PUBGSharp.Net.Model.StatsResponse stats, PUBGSharp.Data.Mode mode)
        {
            InfoToRender toRender = new InfoToRender();
            toRender.m_Stats = new Tuple<string, string>[4, 3];

            var modeStats = stats.Stats.Find(x => x.Mode == mode && x.Region == PUBGSharp.Data.Region.AGG);
            var sorted = modeStats.Stats.OrderBy(x => (x.Rank.HasValue) ? x.Rank.Value : int.MaxValue);

            throw new NotImplementedException(); // Seems like stats does not always get all rank - WHY:( need to investigate that
        }

        private InfoToRender FillStatArrayLast(PUBGSharp.Net.Model.StatsResponse stats, PUBGSharp.Data.Mode mode)
        {
            InfoToRender toRender = new InfoToRender();
            toRender.m_Stats = new Tuple<string, string>[4, 3];

            string otherTypeOfModeString ="";
            switch(mode)
            {
                case PUBGSharp.Data.Mode.SoloFpp:
                    otherTypeOfModeString = "FP Solo";
                    break;
                case PUBGSharp.Data.Mode.DuoFpp:
                    otherTypeOfModeString = "FP Duo";
                    break;
                case PUBGSharp.Data.Mode.SquadFpp:
                    otherTypeOfModeString = "FP Squad";
                    break;
            }

            MatchHistoryStat lastMatch = null;
            for(int i = 0; i < stats.MatchHistory.Count; i++)
            {
                if (stats.MatchHistory[i].Mode == otherTypeOfModeString)
                {
                    lastMatch = stats.MatchHistory[i];
                    break;
                }
            }

            toRender.m_Title = $"Last Match";
            if (lastMatch.Rounds > 1)
                toRender.m_Title += "es";
            toRender.m_Title += $" {lastMatch.Mode.ToString()}";
            toRender.m_Date = lastMatch.Updated.ToString("yyyy-MM-dd");
            toRender.m_Rank = lastMatch.RatingRank;
            toRender.m_RankChange = lastMatch.RatingRankChange;
            

            toRender.m_Stats[0, 0] = new Tuple<string, string>("Kills: ", lastMatch.Kills.ToString());
            toRender.m_Stats[0, 1] = new Tuple<string, string>("Headshots: ", lastMatch.Headshots.ToString());
            toRender.m_Stats[0, 2] = new Tuple<string, string>("Assists: ", lastMatch.Assists.ToString());
            toRender.m_Stats[1, 0] = new Tuple<string, string>("DMG: ", lastMatch.Damage.ToString());
            toRender.m_Stats[1, 1] = new Tuple<string, string>("K/D: ", lastMatch.Kd.ToString());
            toRender.m_Stats[1, 2] = new Tuple<string, string>("Traveled: ", lastMatch.MoveDistance.ToString());
            toRender.m_Stats[2, 0] = new Tuple<string, string>("Rounds played: ", lastMatch.Rounds.ToString());
            toRender.m_Stats[2, 1] = new Tuple<string, string>("Top 10s: ", lastMatch.Top10.ToString());
            toRender.m_Stats[2, 2] = new Tuple<string, string>("Survived: ", $"{(int)(lastMatch.TimeSurvived / 60)}m {(int)(lastMatch.TimeSurvived % 60)}s");
            toRender.m_Stats[3, 0] = null;
            toRender.m_Stats[3, 1] = new Tuple<string, string>("Wins: ", lastMatch.Wins.ToString());
            toRender.m_Stats[3, 2] = null;

            return toRender;
        }
    }
}
