using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class SignedUsers
    {
        [JsonProperty("UserIDs")]
        private Dictionary<ulong, bool> m_Users = new Dictionary<ulong, bool>();
        private Mutex m_Mutex = new Mutex();

        public int GetNumUsers()
        {
            int count = 0;

            m_Mutex.WaitOne();
            try
            {
                count = m_Users.Count;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }

            return count;
        }

        public ulong GetRandomUser()
        {
            var rand = new Random();
            ulong user = 0;

            m_Mutex.WaitOne();
            try
            {
                user = m_Users.ElementAt(rand.Next(0, m_Users.Count)).Key;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }

            return user;
        }

        public List<ulong> GetUsers()
        {
            List<ulong> users = null;

            m_Mutex.WaitOne();
            try
            {
                users = new List<ulong>(m_Users.Keys);
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }

            return users;
        }

        public bool AddUser(ulong id)
        {
            bool added = true;

            m_Mutex.WaitOne();
            try
            {
                if (!m_Users.ContainsKey(id))
                    m_Users.Add(id, true);
                else
                    added = false;
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }

            return added;
        }
    }
}
