using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons;
using Dolores.DataClasses;
using Dolores.EventHandlers;
using MySql.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dolores.Modules.Misc
{
    public class LogHandler : IInstallable
    {
        private DiscordSocketClient m_Client;
        IServiceProvider m_Map;
#if !_WINDOWS_
        ulong m_LogChannelId = 356852896559661056;
#else
        ulong m_LogChannelId = 272513888539639818;
#endif
        ulong m_DebugChannelId = 272513888539639818;
        ITextChannel m_LogChannel;
        ITextChannel m_DebugChannel;
        Notifications m_Notifications; 
        ulong m_GuildId = 269960016591716362;
        Data.DBConnection m_DBConnection;

        public Task Install(IServiceProvider map)
        {
            m_Map = map;
            m_Client = m_Map.GetService<DiscordSocketClient>();
            m_Notifications = m_Map.GetService<Notifications>();
            m_LogChannel = m_Client.GetChannel(m_LogChannelId) as ITextChannel;
            m_DebugChannel = m_Client.GetChannel(m_DebugChannelId) as ITextChannel;
            m_DBConnection = Data.DBConnection.Instance();
            m_DBConnection.DatabaseName = "dolores";
            m_DBConnection.UserName = "dolores";
            m_DBConnection.Password = "chuj";

            if (m_DBConnection.IsConnect())
            {
                bool voiceChannelLogsCreated = false;
                bool userStatusLogsCreated = false;
                string query = "SHOW TABLES";
                var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(0) == "VoiceChannelLogs")
                        voiceChannelLogsCreated = true;
                    if (reader.GetString(0) == "UserStatusLogs")
                        userStatusLogsCreated = true;
                }
                reader.Close();

                if (!voiceChannelLogsCreated)
                {
                    query = "CREATE TABLE VoiceChannelLogs ( id INT(11) UNSIGNED AUTO_INCREMENT PRIMARY KEY, date BIGINT NOT NULL, action TEXT NOT NULL, channel TEXT NOT NULL, user TEXT NOT NULL )";
                    cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        m_LogChannel.SendMessageAsync($"Stworzono tablice VoiceChannelLogs");
                    }
                    reader.Close();
                }
                if (!userStatusLogsCreated)
                {
                    query = "CREATE TABLE UserStatusLogs ( id INT(11) UNSIGNED AUTO_INCREMENT PRIMARY KEY, date BIGINT NOT NULL, status TEXT NOT NULL, user TEXT NOT NULL )";
                    cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        m_LogChannel.SendMessageAsync($"Stworzono tablice UserStatusLogs");
                    }
                    reader.Close();
                }
            }

            m_Client.GuildMemberUpdated += GuildMemberUpdated;
            m_Client.UserBanned += UserBanned;
            m_Client.UserJoined += UserJoined;
            m_Client.UserLeft += UserLeft;
            m_Client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            return Task.CompletedTask;
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (after.VoiceChannel != null && after.VoiceChannel.Guild.Id != m_GuildId)
                return;
            if (before.VoiceChannel != null && before.VoiceChannel.Guild.Id != m_GuildId)
                return;

            if (before.VoiceChannel == null && after.VoiceChannel != null)
            {
                if(m_DBConnection.IsConnect())
                {
                    string query = $"INSERT INTO VoiceChannelLogs (date, action, channel, user) VALUES "
                        +$"('{DateTime.Now.Ticks}', 'JOIN', '{after.VoiceChannel.Name}', '{user.Username}')";
                    var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    cmd.ExecuteNonQuery();
                }
                await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {user.Username} połączył się z kanałem głosowym: {after.VoiceChannel.Name}");
            }
            else if(before.VoiceChannel != null && after.VoiceChannel != null && before.VoiceChannel.Name != after.VoiceChannel.Name)
            {
                if (m_DBConnection.IsConnect())
                {
                    string query = $"INSERT INTO VoiceChannelLogs (date, action, channel, user) VALUES "
                        +$"('{DateTime.Now.Ticks}', 'CHANGE', '{after.VoiceChannel.Name}', '{user.Username}')";
                    var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    cmd.ExecuteNonQuery();
                }
                await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {user.Username} zmienił kanał głosowy: {before.VoiceChannel.Name} -> {after.VoiceChannel.Name}");
            }
            else if(before.VoiceChannel != null && after.VoiceChannel == null)
            {
                if (m_DBConnection.IsConnect())
                {
                    string query = $"INSERT INTO VoiceChannelLogs (date, action, channel, user) VALUES "
                        +$"('{DateTime.Now.Ticks}', 'LEAVE', '{before.VoiceChannel.Name}', '{user.Username}')";
                    var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    cmd.ExecuteNonQuery();
                }
                await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {user.Username} rozłączył się z kanału głosowego: {before.VoiceChannel.Name}");
            }
            // Just for DataBase purposes
            if(before.IsSelfMuted != after.IsSelfMuted)
            {
                if (m_DBConnection.IsConnect())
                {
                    string query = $"INSERT INTO UserStatusLogs (date, status, user) VALUES "
                        +$"('{DateTime.Now.Ticks}', '"+(after.IsSelfMuted ? "MUTED":"UNMUTED")+$"', '{user.Username}')";
                    var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    cmd.ExecuteNonQuery();
                }
            }
            if (before.IsSelfDeafened != after.IsSelfDeafened)
            {
                if (m_DBConnection.IsConnect())
                {
                    string query = $"INSERT INTO UserStatusLogs (date, status, user) VALUES "
                        + $"('{DateTime.Now.Ticks}', '" + (after.IsSelfDeafened ? "DEAFENED" : "UNDEAFENED") + $"', '{user.Username}')";
                    var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            if (user.Guild.Id != m_GuildId)
                return;

            await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {user.Username} opuścił serwer");
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            if (user.Guild.Id != m_GuildId)
                return;

            await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {user.Username} dołączył do serwera");
        }

        private async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            if (guild.Id != m_GuildId)
                return;

            await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {user.Username} został zbanowany");
        }

        private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            if(after.Guild.Id != m_GuildId)
                return;
            if(before.Status != after.Status)
            {
                await m_LogChannel.SendMessageAsync($"[{DateTime.Now.ToString("HH:mm:ss")}] {after.Username} zmienił status na: {after.Status.ToString()}");
                if (m_DBConnection.IsConnect())
                {
                    string query = $"INSERT INTO UserStatusLogs (date, status, user) VALUES "
                        + $"('{DateTime.Now.Ticks}', '{after.Status.ToString().ToUpper()}', '{after.Username}')";
                    var cmd = new MySqlCommand(query, m_DBConnection.Connection);
                    cmd.ExecuteNonQuery();
                }

                if (after.Status == UserStatus.Online)
                {
                    ulong id = m_Notifications.ShouldNofity(after.Id);
                    if (id != 0)
                    {
                        var user = m_Client.GetUser(id);
                        var notify = m_Client.GetUser(after.Id);

                        var userChannel = await user.GetOrCreateDMChannelAsync();
                        await userChannel.SendMessageAsync($"{notify.Mention} pojawił się online");
                    }
                }
            }
        }
    }

    namespace Data
    {
        public class DBConnection
        {
            private DBConnection()
            {
            }

            private string databaseName = string.Empty;
            public string DatabaseName
            {
                get { return databaseName; }
                set { databaseName = value; }
            }

            public string UserName { get; set; }
            public string Password { get; set; }
            private MySqlConnection connection = null;
            public MySqlConnection Connection
            {
                get { return connection; }
            }

            private static DBConnection _instance = null;
            public static DBConnection Instance()
            {
                if (_instance == null)
                    _instance = new DBConnection();
                return _instance;
            }

            public bool IsConnect()
            {
                bool result = false;
                /*try
                {
                    if (Connection == null)
                    {
                        if (String.IsNullOrEmpty(databaseName))
                            result = false;
                        string connstring = string.Format("Server=localhost; database={0}; UID={1}; password={2}", databaseName, UserName, Password);
                        connection = new MySqlConnection(connstring);
                        connection.Open();
                        result = true;
                    }
                }
                catch (Exception) { }*/

                return result;
            }

            public void Close()
            {
                connection.Close();
            }
        }
    }
}
