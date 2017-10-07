using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class Reactions : IState
    {
        private Dictionary<ulong, HashSet<string>> m_Reactions = new Dictionary<ulong, HashSet<string>>();
        private Mutex m_Mutex = new Mutex();

        public void Add(ulong user, params string[] reactions)
        {
            m_Mutex.WaitOne();
            try
            {
                if (!m_Reactions.ContainsKey(user))
                {
                    m_Reactions.Add(user, new HashSet<string>());
                }

                foreach(var reaction in reactions)
                    m_Reactions[user].Add(reaction);
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public void ClearUser(ulong user)
        {
            m_Mutex.WaitOne();
            try
            {
                m_Reactions.Remove(user);
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public void Remove(ulong user, params string[] reactions)
        {
            m_Mutex.WaitOne();
            try
            {
                if (m_Reactions.ContainsKey(user))
                {
                    foreach(var reaction in reactions)
                    {
                        if (m_Reactions[user].Contains(reaction))
                            m_Reactions[user].Remove(reaction);
                    }
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public string[] GetReactions(ulong user)
        {
            List<string> retValue = new List<string>();

            m_Mutex.WaitOne();
            try
            {
                if (m_Reactions.ContainsKey(user))
                {
                    foreach (var reaction in m_Reactions[user])
                        retValue.Add(reaction);
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();

            return retValue.ToArray();
        }

        public void Save()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("reactions.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_Reactions));
                    streamWriter.Flush();
                    stream.Flush();
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public void Load()
        {
            m_Mutex.WaitOne();
            try
            {
                using (Stream stream = File.Open("reactions.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    m_Reactions = JsonConvert.DeserializeObject<Dictionary<ulong, HashSet<string>>>(streamReader.ReadLine());
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
