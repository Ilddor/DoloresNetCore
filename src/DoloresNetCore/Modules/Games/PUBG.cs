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
                //graphics.CompositingMode = CompositingMode.SourceCopy;

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

                // Draw player name
                Font drawFont = new Font("Arial", 20);
                String playerName = stats.PlayerName;
                graphics.DrawString(stats.PlayerName, drawFont, Brushes.White, avatar.Width + startX, startY);

                string typeString = "Last Day Played";
                var typeSize = graphics.MeasureString(typeString, drawFont);
                graphics.DrawString(typeString, drawFont, Brushes.White, image.Width / 2 - typeSize.Width / 2, startY);

                var lastMatch = stats.MatchHistory[0];

                int posY = startY + avatar.Height + 30;
                graphics.DrawString($"Assists: {lastMatch.Assists}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"DMG: {lastMatch.Damage}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Kills: {lastMatch.Kills}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Traveled: {lastMatch.MoveDistance}m", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Rounds played: {lastMatch.Rounds}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Top 10s: {lastMatch.Top10}", drawFont, Brushes.White, startX, posY);
                posY += 40;
                graphics.DrawString($"Wins: {lastMatch.Wins}", drawFont, Brushes.White, startX, posY);

                graphics.Save();
                var fileOutput = File.Open($"{name}.png", FileMode.OpenOrCreate);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
                var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
                image.Save(fileOutput, codec, encoderParameters);
                fileOutput.Close();
            }
            await Context.Channel.SendFileAsync($"{name}.png");
        }
    }
}
