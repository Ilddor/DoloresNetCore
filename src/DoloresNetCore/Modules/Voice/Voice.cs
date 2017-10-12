using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Audio;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Dolores.DataClasses;
using Dolores.CustomAttributes;

namespace Dolores.Modules.Voice
{
    [RequireInstalled]
    [LangSummary(LanguageDictionary.Language.PL, "Dodaje możliwość dołączania do kanału oraz puszczania muzyki")]
    [LangSummary(LanguageDictionary.Language.EN, "Adds possibility to join voice channels and playing music")]
    public class Voice : ModuleBase
    {
        private IServiceProvider m_Map;

        public class AudioClientWrapper
        {
            public IAudioClient m_AudioClient = null;
            public Process m_Process = null;
            public bool m_Playing = false;
            public IVoiceChannel m_CurrentChannel = null;

            public async Task JoinVoiceChannel(IServiceProvider map, IVoiceChannel channel)
            {
                //AudioClientWrapper audioClient = map.GetService<AudioClientWrapper>();
                if (m_AudioClient != null)
                {
                    if (m_Playing)
                    {
                        StopPlay(map);
                    }
                    await m_AudioClient.StopAsync();
                    m_AudioClient = null;
                }

                m_CurrentChannel = channel;
                m_AudioClient = await channel.ConnectAsync();
            }

            public async Task LeaveVoiceChannel(IServiceProvider map)
            {
                AudioClientWrapper audioClient = map.GetService<AudioClientWrapper>();
                if (audioClient.m_AudioClient != null)
                {
                    if (m_Playing)
                    {
                        StopPlay(map);
                    }
                    m_CurrentChannel = null;
                    await audioClient.m_AudioClient.StopAsync();
                }
            }

            public void StopPlay(IServiceProvider map)
            {
                if (m_Process != null)
                {
                    m_Process.Kill();
                    m_Process.WaitForExit();
                    m_Playing = false;
                    m_Process = null;
                }
            }
        }

        public Voice(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("joinAudio", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Powoduje że bot dołącza do twojego kanału głosowego")]
        [LangSummary(LanguageDictionary.Language.EN, "Bot will join your voice channel")]
        private async Task JoinAudio()
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
            await m_Map.GetService<AudioClientWrapper>().JoinVoiceChannel(m_Map, channel);
        }

        [Command("leaveAudio", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Powoduje że bot wychodzi z kanału głosowego")]
        [LangSummary(LanguageDictionary.Language.EN, "Bot will leave voice channel")]
        private async Task LeaveAudio()
        {
            await m_Map.GetService<AudioClientWrapper>().LeaveVoiceChannel(m_Map);
        }

        [Command("westworld", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Odtawrza Westworld Main Theme")]
        [LangSummary(LanguageDictionary.Language.EN, "Plays Westworld Main Theme")]
        private async Task Westworld()
        {
            {
                var ffmpeg = CreateStream("WestworldMainTheme.mp3");
                AudioClientWrapper audioWrapper = m_Map.GetService<AudioClientWrapper>();
                audioWrapper.m_Process = ffmpeg;

                var output = audioWrapper.m_Process.StandardOutput.BaseStream;
                var discord = m_Map.GetService<AudioClientWrapper>().m_AudioClient.CreatePCMStream(AudioApplication.Music, 1920);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
                audioWrapper.StopPlay(m_Map);
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        [LangSummary(LanguageDictionary.Language.PL, "Podaj link jako parametr, a zagra muzyke z tego linka(youtube)")]
        [LangSummary(LanguageDictionary.Language.EN, "When a youtube link is given, bot will start to play a song from that link")]
        private async Task Play(string url)
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);
            AudioClientWrapper tmp = m_Map.GetService<AudioClientWrapper>();
            if (tmp.m_Playing)
            {
                await Context.Channel.SendMessageAsync($"{guildConfig.Translation.CurrentlyPlaying}");
                return;
            }

            string name = "/home/ilddor/Music/" + url.Substring(url.Length - 11, 11) + ".mp3";
            if (!System.IO.File.Exists(name))
            {
                var ytd = new ProcessStartInfo
                {
                    FileName = "youtube-dl",
                    Arguments = $"--extract-audio --audio-format mp3 -o ~/Music/%(id)s.%(ext)s {url}",
                    //Arguments = $"-i \"https://www.youtube.com/watch?v=8w_lwezZDUw \" -af \"volume=0.1\" -ac 2 -f s16le -ar 44000 pipe:1",
                    UseShellExecute = false,

                    RedirectStandardOutput = true,
                };
                Process ytdl = Process.Start(ytd);

                ytdl.WaitForExit();
            }

            
            await Context.Channel.SendMessageAsync($"{guildConfig.Translation.StartingPlaying}");
            {
                var ffmpeg = CreateStream(name);
                tmp.m_Process = ffmpeg;
                tmp.m_Playing = true;
                var output = tmp.m_Process.StandardOutput.BaseStream;
                var discord = m_Map.GetService<AudioClientWrapper>().m_AudioClient.CreatePCMStream(AudioApplication.Music, 1920);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
                tmp.StopPlay(m_Map);
                await Context.Channel.SendMessageAsync($"{guildConfig.Translation.SongEnd}");
            }
        }

        [Command("stopPlay")]
        [LangSummary(LanguageDictionary.Language.PL, "Przerywa wykonywanie aktualnego utworu jeśli jest odtwarzany")]
        [LangSummary(LanguageDictionary.Language.EN, "Stops playing current song")]
        private async Task StopPlay()
        {
            var configs = m_Map.GetService<Configurations>();
            Configurations.GuildConfig guildConfig = configs.GetGuildConfig(Context.Guild.Id);
            AudioClientWrapper audioWrapper = m_Map.GetService<AudioClientWrapper>();
            if (audioWrapper.m_Process != null)
            {
                //process.m_Process.Dispose();
                //process.m_Process.StandardInput.WriteLine("\x3");
                audioWrapper.StopPlay(m_Map);
                await Context.Channel.SendMessageAsync($"{guildConfig.Translation.StoppingPlaying}: {audioWrapper.m_Process.HasExited}");
            }
        }

        private Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path} -af \"volume=0.1\" -ac 2 -f s16le pipe:1",
                //Arguments = $"-i \"https://www.youtube.com/watch?v=8w_lwezZDUw \" -af \"volume=0.1\" -ac 2 -f s16le -ar 44000 pipe:1",
                UseShellExecute = false,
                
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
            };
            return Process.Start(ffmpeg);
        }
    }
}
