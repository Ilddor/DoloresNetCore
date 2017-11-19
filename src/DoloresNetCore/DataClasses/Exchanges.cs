using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Dolores.DataClasses
{
    public class Exchanges : IState
    {
        private class Exchange
        {
            public ulong m_HelperMessageID = 0;
            public ulong m_ChannelID = 0;
            public ulong m_GuildID = 0;
            public string m_SignInReaction = null;
            public Dictionary<ulong, string> m_Addresses = new Dictionary<ulong, string>();
        }

        private Dictionary<ulong, Exchange> m_Exchanges = new Dictionary<ulong, Exchange>();
        private Mutex m_Mutex = new Mutex();

        public void AddAddress(ulong exchangeID, ulong userID, string address)
        {
            m_Mutex.WaitOne();
            try
            {
                if (!m_Exchanges.ContainsKey(exchangeID))
                    m_Exchanges.Add(exchangeID, new Exchange());

                if (!m_Exchanges[exchangeID].m_Addresses.ContainsKey(userID))
                    m_Exchanges[exchangeID].m_Addresses.Add(userID, address);
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }

        public void AddExchange(ulong exchangeID)
        {
            m_Mutex.WaitOne();
            try
            {
                if (!m_Exchanges.ContainsKey(exchangeID))
                    m_Exchanges.Add(exchangeID, new Exchange());
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }

        public void AddHelperMessage(ulong exchangeID, ulong helperMessageID)
        {
            m_Mutex.WaitOne();
            try
            {
                m_Exchanges[exchangeID].m_HelperMessageID = helperMessageID;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }

        public void AddChannelID(ulong exchangeID, ulong channelID)
        {
            m_Mutex.WaitOne();
            try
            {
                m_Exchanges[exchangeID].m_ChannelID = channelID;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }

        public void AddGuildID(ulong exchangeID, ulong guildID)
        {
            m_Mutex.WaitOne();
            try
            {
                m_Exchanges[exchangeID].m_GuildID = guildID;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }

        public void AddSignInReaction(ulong exchangeID, string signInReaction)
        {
            m_Mutex.WaitOne();
            try
            {
                m_Exchanges[exchangeID].m_SignInReaction = signInReaction;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }

        public bool IsExchange(ulong exchangeID)
        {
            bool result = false;
            m_Mutex.WaitOne();
            try
            {
                if (m_Exchanges.ContainsKey(exchangeID))
                    result = true;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
            return result;
        }

        public string GetUserAddress(ulong exchangeID, ulong userID)
        {
            string result = null;
            m_Mutex.WaitOne();
            try
            {
                if (m_Exchanges.ContainsKey(exchangeID))
                {
                    if (m_Exchanges[exchangeID].m_Addresses.ContainsKey(userID))
                        result = m_Exchanges[exchangeID].m_Addresses[userID];
                }
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
            return result;
        }

        public string GetSignInReaction(ulong exchangeID)
        {
            string result = null;
            m_Mutex.WaitOne();
            try
            {
                if (m_Exchanges.ContainsKey(exchangeID))
                {
                    result = m_Exchanges[exchangeID].m_SignInReaction;
                }
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
            return result;
        }

        public ulong GetHelperMessageID(ulong exchangeID)
        {
            ulong result = 0;
            m_Mutex.WaitOne();
            try
            {
                if (m_Exchanges.ContainsKey(exchangeID))
                {
                    result = m_Exchanges[exchangeID].m_HelperMessageID;
                }
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
            return result;
        }

        public ulong GetChannelID(ulong exchangeID)
        {
            ulong result = 0;
            m_Mutex.WaitOne();
            try
            {
                if (m_Exchanges.ContainsKey(exchangeID))
                {
                    result = m_Exchanges[exchangeID].m_ChannelID;
                }
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
            return result;
        }

        public ulong GetGuildID(ulong exchangeID)
        {
            ulong result = 0;
            m_Mutex.WaitOne();
            try
            {
                if (m_Exchanges.ContainsKey(exchangeID))
                {
                    result = m_Exchanges[exchangeID].m_GuildID;
                }
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
            return result;
        }

        public void Load()
        {
            m_Mutex.WaitOne();
            try
            {
                using (Stream stream = File.Open("exchanges.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    m_Exchanges = JsonConvert.DeserializeObject<Dictionary<ulong, Exchange>>(streamReader.ReadToEnd());
                }
            }
            catch (IOException) { }
            m_Mutex.ReleaseMutex();
        }

        public void Save()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("exchanges.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_Exchanges, Formatting.Indented));
                    streamWriter.Flush();
                    stream.Flush();
                }
            }
            catch (IOException) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
