using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dolores.DataClasses;
using Dolores.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Drawing.Imaging;

namespace Dolores.Modules.Games
{
    [RequireInstalled]
    [Group("GameTime")]
    [LangSummary(LanguageDictionary.Language.PL, "Pozwala wyświetlać statystyki czasu gry użytkowników tego serwera")]
    [LangSummary(LanguageDictionary.Language.EN, "Allows to show game time statistics of users of this server")]
    public class GameTime : ModuleBase
    {
        public enum StatType
        {
            TopGames,
            TopUsers,
            //GuildTopGames,
            //GuildTopUsers
        }

        IServiceProvider m_Map;

        public GameTime(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("topGames", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Pokazuje czas gry spędzony w konkretnych grach zsumowany dla wszystkich serwerów na których znajduje się bot")]
        [LangSummary(LanguageDictionary.Language.EN, "Shows game time spent for each game recorded on all servers that bot is on")]
        public async Task TopGames(int numTopResults = 10)
        {
            Bitmap image = DrawBitmap(StatType.TopGames, numTopResults);

            var fileOutput = File.Open($"RTResources/Images/GameTime.png", FileMode.OpenOrCreate);
            try
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
                var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
                image.Save(fileOutput, codec, encoderParameters);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            fileOutput.Close();

            var command = Context.Message.Content;
            Context.Message.DeleteAsync();
            await Context.Channel.SendFileAsync($"RTResources/Images/GameTime.png", text: $"Command: `{command}`");
        }

        [Command("topGamesGuild", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Pokazuje czas gry spędzony w konkretnych grach zsumowany dla tego serwera")]
        [LangSummary(LanguageDictionary.Language.EN, "Shows game time spent for each game recorded on this server")]
        public async Task TopGamesGuild(int numTopResults = 10)
        {
            var users = await Context.Guild.GetUsersAsync();
            var list = users.Select(x => x.Id);
            Bitmap image = DrawBitmap(StatType.TopGames, numTopResults, list);

            var fileOutput = File.Open($"RTResources/Images/GameTime.png", FileMode.OpenOrCreate);
            try
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75);
                var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
                image.Save(fileOutput, codec, encoderParameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            fileOutput.Close();

            var command = Context.Message.Content;
            Context.Message.DeleteAsync();
            await Context.Channel.SendFileAsync($"RTResources/Images/GameTime.png", text: $"Command: `{command}`");
        }

        private Bitmap DrawBitmap(StatType type, int numTopResults, IEnumerable<ulong> userSet = null)
        {
            var gameTimes = m_Map.GetService<GameTimes>();
            var client = m_Map.GetService<DiscordSocketClient>();
            Bitmap image = null;

            var list = gameTimes.GetTopGames(numTopResults, userSet);
            IEnumerable<string> printList = null;
            switch(type)
            {
                case StatType.TopGames:
                    printList = GetPrintStringsTopGames(list);
                    break;
                case StatType.TopUsers:
                    printList = GetPrintStringsTopUsers(list);
                    break;
            }

            try
            {
                // TODO: think of copying DB outside mutex scope to avoid long locks
                int startX = 10, startY = 10;
                Font drawFont = new Font(SystemFonts.DefaultFont.FontFamily, 18, FontStyle.Regular, GraphicsUnit.Point);
                image = new Bitmap(640, (int)((drawFont.Height) * (numTopResults + 1)) + 20);

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

                foreach (var row in printList)
                {
                    graphics.DrawString(row, drawFont, Brushes.White, posX, posY);
                    posY += drawFont.Size + 10;
                }

                graphics.Save();
                drawFont.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return image;
        }

        private IEnumerable<string> GetPrintStringsTopGames(IEnumerable<KeyValuePair<string,long>> results)
        {
            return results.Select(x =>
            {
                string line = $"{x.Key} ";
                TimeSpan time = new TimeSpan(x.Value);
                if (time.Days != 0)
                    line += $"{time.Days}d ";
                line += $"{time.ToString(@"hh\:mm\:ss")}\n";
                return line;
            });
        }

        private IEnumerable<string> GetPrintStringsTopUsers(IEnumerable<KeyValuePair<string, long>> results)
        {
            throw new NotImplementedException();
        }
    }
}
