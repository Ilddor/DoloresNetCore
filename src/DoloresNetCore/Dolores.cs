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
using Dolores.Modules.Social;
using Dolores.Modules.Games;
using Dolores.Modules.Voice;
using Dolores.Modules.Misc;
using Dolores.DataClasses;

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
            AssemblyLoadContext.Default.Unloading += ClosingEvent;
            m_Instance = new Dolores();
            Console.CancelKeyPress += Console_CancelKeyPress;
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
        public DependencyMap map = new DependencyMap();
        public APIKeys m_APIKeys;

        private DiscordSocketClient m_Client;
        private CommandHandler m_CommandHandler;

        CreatedChannels m_CreatedChannels;
        Social          m_SocialModule;
        SignedUsers     m_SignedUsers;
        GameTimes       m_GameTimes;
        Reactions       m_Reactions;
        Notifications   m_Notifications;

        Logging         m_Logging;

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
            map.Add(m_Client);

            m_CreatedChannels = new CreatedChannels();
            m_SignedUsers = new SignedUsers();
            m_GameTimes = new GameTimes();
            m_Reactions = new Reactions();
            m_Notifications = new Notifications();
            LoadState();

            map.Add(m_CreatedChannels);
            map.Add(m_SignedUsers);
            map.Add(m_GameTimes);
            map.Add(m_Reactions);
            map.Add(m_Notifications);
            map.Add(m_APIKeys);

            GameChannels.Install(map);
            GameTime.Install(map);

            m_CommandHandler = new CommandHandler();
            await m_CommandHandler.Install(map);

            m_SocialModule = new Social(map);
            m_SocialModule.Install(map);

            m_Logging = new Logging();
            m_Logging.Install(map);
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

            Voice.AudioClientWrapper audioClient;
            if (map.TryGet(out audioClient))
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
