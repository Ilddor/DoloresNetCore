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

namespace Dolores.Modules.Voice
{
    public class Voice : ModuleBase
    {
        private IDependencyMap m_Map;

        public class AudioClientWrapper
        {
            public IAudioClient m_AudioClient;
        }

        public class FFMPEGProcess
        {
            public Process m_Process;
            public bool m_Playing = false;

            public FFMPEGProcess(Process process)
            {
                m_Process = process;
            }
        }

        public Voice(IDependencyMap map)
        {
            m_Map = map;
        }

        [Command("joinAudio", RunMode = RunMode.Async)]
        [Summary("Powoduje że bot dołącza do twojego kanału głosowego")]
        private async Task JoinAudio()
        {
            //if (Context.User.Username == "Ilddor")
            {
                IVoiceChannel channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
                AudioClientWrapper audioClient;
                if (!m_Map.TryGet<AudioClientWrapper>(out audioClient))
                {
                    audioClient = new AudioClientWrapper();
                    m_Map.Add(audioClient);
                }
                else
                {
                    FFMPEGProcess ffmpegProcess;
                    if (m_Map.TryGet(out ffmpegProcess))
                    {
                        if (ffmpegProcess.m_Playing)
                        {
                            await StopPlay();
                        }
                    }
                    await audioClient.m_AudioClient.StopAsync();
                }
                audioClient.m_AudioClient = await channel.ConnectAsync();
            }
        }

        [Command("leaveAudio", RunMode = RunMode.Async)]
        [Summary("Powoduje że bot wychodzi z kanału głosowego")]
        private async Task LeaveAudio()
        {
            AudioClientWrapper audioClient;
            if (m_Map.TryGet<AudioClientWrapper>(out audioClient))
            {

                FFMPEGProcess ffmpegProcess;
                if (m_Map.TryGet(out ffmpegProcess))
                {
                    if (ffmpegProcess.m_Playing)
                    {
                        await StopPlay();
                    }
                }
                await audioClient.m_AudioClient.StopAsync();
            }
        }

        [Command("westworld", RunMode = RunMode.Async)]
        [Summary("Odtawrza Westworld Main Theme")]
        private async Task Westworld()
        {
            //if (Context.User.Username == "Ilddor")
            {
                var ffmpeg = CreateStream("WestworldMainTheme.mp3");
                FFMPEGProcess ffmpegProcess;
                if (!m_Map.TryGet(out ffmpegProcess))
                {
                    ffmpegProcess = new FFMPEGProcess(ffmpeg);
                    m_Map.Add(ffmpegProcess);
                }
                else
                {
                    ffmpegProcess.m_Process = ffmpeg;
                }
                var output = ffmpegProcess.m_Process.StandardOutput.BaseStream;
                var discord = m_Map.Get<AudioClientWrapper>().m_AudioClient.CreatePCMStream(AudioApplication.Music, 1920);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
                //ffmpeg.Close();
                ffmpegProcess.m_Process.Kill();
                ffmpegProcess.m_Process.WaitForExit();
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
            FFMPEGProcess tmp;
            if (m_Map.TryGet(out tmp))
            {
                if (tmp.m_Playing)
                {
                    await Context.Channel.SendMessageAsync("Aktualnie odtwarzana jest muzyka, kolejka nie jest jeszcze wspierana, by zmienic utwór wpisz najpierw !stopPlay");
                    return;
                }
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
                FFMPEGProcess ffmpegProcess;
                if(!m_Map.TryGet(out ffmpegProcess))
                {
                    ffmpegProcess = new FFMPEGProcess(ffmpeg);
                    m_Map.Add(ffmpegProcess);
                }
                else
                {
                    ffmpegProcess.m_Process = ffmpeg;
                }
                ffmpegProcess.m_Playing = true;
                var output = ffmpegProcess.m_Process.StandardOutput.BaseStream;
                var discord = m_Map.Get<AudioClientWrapper>().m_AudioClient.CreatePCMStream(AudioApplication.Music, 1920);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
                //ffmpeg.Close();
                //ffmpegProcess.m_Process.Kill();
                //ffmpegProcess.m_Process.Dispose();
                ffmpegProcess.m_Process.WaitForExit();
                ffmpegProcess.m_Playing = false;
                await Context.Channel.SendMessageAsync("Koniec utworu");
            }
        }

        [Command("stopPlay")]
        [Summary("Przerywa wykonywanie aktualnego utworu jeśli jest odtwarzany")]
        private async Task StopPlay()
        {
            FFMPEGProcess process;
            if(m_Map.TryGet(out process))
            {
                //process.m_Process.Dispose();
                //process.m_Process.StandardInput.WriteLine("\x3");
                process.m_Process.Kill();
                process.m_Process.WaitForExit();
                process.m_Playing = false;
                await Context.Channel.SendMessageAsync($"Przerywam odtwarzanie, proces sie zakonczyl: {process.m_Process.HasExited}");
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
