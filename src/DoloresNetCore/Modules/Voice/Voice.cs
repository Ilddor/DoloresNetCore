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

namespace Dolores.Modules.Voice
{
    public class Voice : ModuleBase
    {
        private IServiceProvider m_Map;

        public class AudioClientWrapper
        {
            public IAudioClient m_AudioClient = null;
            public Process m_Process = null;
            public bool m_Playing = false;

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
                }

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

        /*public class FFMPEGProcess
        {
            public Process m_Process = null;
            public bool m_Playing = false;

            public FFMPEGProcess(Process process)
            {
                m_Process = process;
            }
        }*/

        public Voice(IServiceProvider map)
        {
            m_Map = map;
        }

        [Command("joinAudio", RunMode = RunMode.Async)]
        [Summary("Powoduje że bot dołącza do twojego kanału głosowego")]
        private async Task JoinAudio()
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
            await m_Map.GetService<AudioClientWrapper>().JoinVoiceChannel(m_Map, channel);
        }

        [Command("leaveAudio", RunMode = RunMode.Async)]
        [Summary("Powoduje że bot wychodzi z kanału głosowego")]
        private async Task LeaveAudio()
        {
            await m_Map.GetService<AudioClientWrapper>().LeaveVoiceChannel(m_Map);
            //await AudioClientWrapper.LeaveVoiceChannel(m_Map);
        }

        [Command("westworld", RunMode = RunMode.Async)]
        [Summary("Odtawrza Westworld Main Theme")]
        private async Task Westworld()
        {
            //if (Context.User.Username == "Ilddor")
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
            /*WaveFormat outFormat = new WaveFormat(48000, 16, m_audio.Config.Channels);
            using (Mp3FileReader reader = new Mp3FileReader("WestworldMainTheme.mp3"))
            using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, outFormat))
            {
                resampler.ResamplerQuality = 60;
                int blockSize = outFormat.AverageBytesPerSecond / 50;
                byte[] buffer = new byte[blockSize];
                int byteCount;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                {
                    if (byteCount < blockSize)
                    {
                        for (int i = byteCount; i < blockSize; i++)
                        {
                            buffer[i] = 0;
                        }
                    }
                    ScaleVolumeSafeNoAlloc(buffer, 1f);

                    m_audioClient.Send(buffer, 0, blockSize);
                }
            }
            return e.Channel.SendMessage("Odtwarzanie: Westworld Main Theme");*/
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Podaj link jako parametr, a zagra muzyke z tego linka(youtube)")]
        private async Task Play(string url)
        {
            AudioClientWrapper tmp = m_Map.GetService<AudioClientWrapper>();
            if (tmp.m_Playing)
            {
                await Context.Channel.SendMessageAsync("Aktualnie odtwarzana jest muzyka, kolejka nie jest jeszcze wspierana, by zmienic utwór wpisz najpierw !stopPlay");
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

            
            await Context.Channel.SendMessageAsync("Właczam odtwarzanie");
            {
                var ffmpeg = CreateStream(name);
                tmp.m_Process = ffmpeg;
                tmp.m_Playing = true;
                var output = tmp.m_Process.StandardOutput.BaseStream;
                var discord = m_Map.GetService<AudioClientWrapper>().m_AudioClient.CreatePCMStream(AudioApplication.Music, 1920);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
                tmp.StopPlay(m_Map);
                await Context.Channel.SendMessageAsync("Koniec utworu");
            }
        }

        [Command("stopPlay")]
        [Summary("Przerywa wykonywanie aktualnego utworu jeśli jest odtwarzany")]
        private async Task StopPlay()
        {
            AudioClientWrapper audioWrapper = m_Map.GetService<AudioClientWrapper>();
            if (audioWrapper.m_Process != null)
            {
                //process.m_Process.Dispose();
                //process.m_Process.StandardInput.WriteLine("\x3");
                audioWrapper.StopPlay(m_Map);
                await Context.Channel.SendMessageAsync($"Przerywam odtwarzanie, proces sie zakonczyl: {audioWrapper.m_Process.HasExited}");
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
