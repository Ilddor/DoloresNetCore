using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using System.Threading;
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
                    var channel = (ITextChannel)Dolores.m_Instance.m_Client.GetChannel(357908791745839104);
                    var message = await channel.GetMessageAsync(358726726513065985);
                    var context = new CommandContext(Dolores.m_Instance.m_Client, (IUserMessage)message);
                    await Dolores.m_Instance.m_CommandHandler.m_Commands.ExecuteAsync(context, 1, Dolores.m_Instance.map);
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
        //public DependencyMap map = new DependencyMap();
        public IServiceProvider map;

        public APIKeys m_APIKeys;

        private DiscordSocketClient m_Client;
        private CommandHandler m_CommandHandler;

        CreatedChannels  m_CreatedChannels;
        Social           m_SocialModule;
        ForeverAlone     m_ForeverAlone;
        SignedUsers      m_SignedUsers;
        GameTimes        m_GameTimes;
        Reactions        m_Reactions;
        Notifications    m_Notifications;
        BannedSubreddits m_BannedSubreddits;

        Logging         m_Logging;

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(m_Client);

            m_CreatedChannels = new CreatedChannels();
            m_SignedUsers = new SignedUsers();
            m_GameTimes = new GameTimes();
            m_Reactions = new Reactions();
            m_Notifications = new Notifications();
            m_BannedSubreddits = new BannedSubreddits();
            LoadState();

            services.AddSingleton(m_CreatedChannels);
            services.AddSingleton(m_SignedUsers);
            services.AddSingleton(m_GameTimes);
            services.AddSingleton(m_Reactions);
            services.AddSingleton(m_Notifications);
            services.AddSingleton(m_BannedSubreddits);
            services.AddSingleton(m_APIKeys);

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

            m_APIKeys = APIKeys.LoadKeys();

            await m_Client.LoginAsync(TokenType.Bot, m_APIKeys.DiscordAPIKey);
            await m_Client.StartAsync();

            m_Client.Connected += Connected;

            await Task.Delay(-1);
        }

        private async Task Connected()
        {
            /*var services = new ServiceCollection();
            services.AddSingleton(m_Client);

            m_CreatedChannels = new CreatedChannels();
            m_SignedUsers = new SignedUsers();
            m_GameTimes = new GameTimes();
            m_Reactions = new Reactions();
            m_Notifications = new Notifications();
            LoadState();

            services.AddSingleton(m_CreatedChannels);
            services.AddSingleton(m_SignedUsers);
            services.AddSingleton(m_GameTimes);
            services.AddSingleton(m_Reactions);
            services.AddSingleton(m_Notifications);
            services.AddSingleton(m_APIKeys);*/
            if (!m_Installed)
            {
                map = ConfigureServices();

                GameChannels.Install(map);
                GameTime.Install(map);

                m_CommandHandler = new CommandHandler();
                await m_CommandHandler.Install(map);

                m_SocialModule = new Social(map);
                m_SocialModule.Install(map);

                m_ForeverAlone = new ForeverAlone();
                m_ForeverAlone.Install(map);

                m_Logging = new Logging();
                m_Logging.Install(map);

                m_Installed = true;
            }
        }

        public async Task SaveState()
        {
            await m_Client.SetStatusAsync(UserStatus.Invisible);
            // Channels
            m_CreatedChannels.SaveToFile();
            // Signed Users
            m_SignedUsers.SaveToFile();
            // Game Times
            m_GameTimes.SaveToFile();
            // Reactions
            m_Reactions.SaveToFile();
            // Notifications
            m_Notifications.SaveToFile();
            // Banned subreddits
            m_BannedSubreddits.SaveToFile();

            Voice.AudioClientWrapper audioClient = map.GetService<Voice.AudioClientWrapper>();
            if (audioClient.m_AudioClient != null)
            {
                await audioClient.m_AudioClient.StopAsync();
            }

            await m_Client.StopAsync();
        }

        public void LoadState()
        {
            // Channels
            m_CreatedChannels.LoadFromFile();
            // Signed Users
            m_SignedUsers.LoadFromFile();
            // Game Times
            m_GameTimes.LoadFromFile();
            // Reactions
            m_Reactions.LoadFromFile();
            // Notifications
            m_Notifications.LoadFromFile();
            // Banned subreddits
            m_BannedSubreddits.LoadFromFile();

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
