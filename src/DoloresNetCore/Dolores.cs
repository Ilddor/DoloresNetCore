using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Runtime.Loader;
using System.Xml.Serialization;
using Dolores.Modules.Games;
using Dolores.Modules.Voice;
using Dolores.Modules.Social;
using Dolores.Modules.Misc;
using Dolores.DataClasses;
using Dolores.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores
{
    class Dolores
    {
        static void Main(string[] args)
        {
#if _WINDOWS_
            handler = new HandlerRoutine(ConsoleCtrlCheck);
            SetConsoleCtrlHandler(handler, true);
#endif
#if DEBUG
            Thread ConsoleKeyListener = new Thread(new ThreadStart(ListerKeyBoardEvent));
#endif
            AssemblyLoadContext.Default.Unloading += ClosingEvent;
            m_Instance = new Dolores();
            Console.CancelKeyPress += Console_CancelKeyPress;
#if DEBUG
            ConsoleKeyListener.Start();
#endif
            m_Instance.Start().GetAwaiter().GetResult();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Dolores.m_Instance.SaveState().GetAwaiter().GetResult();
        }

        public static void ClosingEvent(AssemblyLoadContext context)
        {
            Dolores.m_Instance.SaveState().GetAwaiter().GetResult();
        }

#if DEBUG
        public static async void ListerKeyBoardEvent()
        {
            do
            {
                if (Console.ReadKey(true).Key == ConsoleKey.D)
                {
                    var channel = (ITextChannel)Dolores.m_Instance.m_Client.GetChannel(272419366744883200);
                    var message = await channel.GetMessageAsync(364517733808865281);
                    var context = new CommandContext(Dolores.m_Instance.m_Client, (IUserMessage)message);
                    await ((CommandHandler)Dolores.m_Instance.m_Handlers.Find(x => x.GetType() == typeof(CommandHandler))).m_Commands.ExecuteAsync(context, 1, Dolores.m_Instance.m_Map);
                }
            } while (true);
        }
#endif
#if _WINDOWS_
        private static bool ConsoleCtrlCheck(CtrlTypes eventType)
        {
            //if (eventType == 2)
            switch(eventType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    Console.WriteLine("Console window closing");
                Dolores.m_Instance.SaveState().GetAwaiter().GetResult();
                    break;
            }
            return true;
        }

        static HandlerRoutine handler;   // Keeps it from getting garbage collected
        // Pinvoke
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes eventType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
#endif

        //*********************************************************************************

        public static Dolores m_Instance;
        public DateTime m_StartTime = DateTime.Now;
        public int m_Version = 0;
        public bool m_Installed = false;
        public IServiceProvider m_Map;

        private DiscordSocketClient m_Client;

        private List<IInstallable> m_Handlers;
        private List<IState> m_DataContainers;

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(m_Client);

            services.AddSingleton(this);

            foreach(var dataClass in m_DataContainers)
            {
                services.AddSingleton(dataClass.GetType(), dataClass);
            }

            services.AddSingleton<Voice.AudioClientWrapper>();

            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            provider.GetService<DiscordSocketClient>();
            return provider;
        }

        public async Task Start()
        {
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.ConnectionTimeout = 120000;
            config.LogLevel = LogSeverity.Debug;

            m_Client = new DiscordSocketClient(config);

            m_Client.Log += Log;

            LoadState();

            m_Map = ConfigureServices();

            await m_Client.LoginAsync(TokenType.Bot, m_Map.GetService<APIKeys>().DiscordAPIKey);
            await m_Client.StartAsync();

            m_Client.Connected += Connected;

            await Task.Delay(-1);
        }

        private async Task Connected()
        {
            if (!m_Installed)
            {
                // Move it to configuration when configuration/installation command is finished
                Directory.CreateDirectory("RTResources");
                Directory.CreateDirectory("RTResources/Images");
                Directory.CreateDirectory("RTResources/Images/StatsPUBG");
                Directory.CreateDirectory("RTResources/Images/StatsCSGO");

                // Create and install all IInstallable modules
                m_Handlers = new List<IInstallable>();
                foreach(var handler in Assembly.GetEntryAssembly().DefinedTypes)
                {
                    if(handler.ImplementedInterfaces.Contains(typeof(IInstallable)))
                    {
                        m_Handlers.Add((IInstallable)Activator.CreateInstance(handler));
                        m_Handlers.Last().Install(m_Map);
                    }
                }

                m_Installed = true;
            }
        }

        public async Task SaveState()
        {
            await m_Client.SetStatusAsync(UserStatus.Invisible);

            foreach(var dataClass in m_DataContainers)
            {
                dataClass.Save();
            }

            Voice.AudioClientWrapper audioClient = m_Map.GetService<Voice.AudioClientWrapper>();
            if (audioClient.m_AudioClient != null)
            {
                await audioClient.m_AudioClient.StopAsync();
            }

            await m_Client.StopAsync();
        }

        public void LoadState()
        {
            // Create and load all DataClasses implementing IState
            m_DataContainers = new List<IState>();
            foreach (var handler in Assembly.GetEntryAssembly().DefinedTypes)
            {
                if (handler.ImplementedInterfaces.Contains(typeof(IState)))
                {
                    m_DataContainers.Add((IState)Activator.CreateInstance(handler));
                    m_DataContainers.Last().Load();
                }
            }

            try
            {
                using (Stream stream = File.Open("version", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    while (!streamReader.EndOfStream)
                    {
                        m_Version = int.Parse(streamReader.ReadLine());
                    }
                }
            }
            catch (IOException) { }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
