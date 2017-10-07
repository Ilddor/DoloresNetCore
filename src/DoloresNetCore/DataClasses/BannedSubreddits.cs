using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

namespace Dolores.DataClasses
{
    class BannedSubreddits : IState
    {
        public HashSet<string> m_Names = new HashSet<string>();
        public Mutex m_Mutex = new Mutex();

        public void Save()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("bannedSubreddits.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_Names));
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
                using (Stream stream = File.Open("bannedSubreddits.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    m_Names = JsonConvert.DeserializeObject<HashSet<string>>(streamReader.ReadLine());
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public bool Contains(string value)
        {
            bool retVal;
            m_Mutex.WaitOne();
            try
            {
                retVal = m_Names.Contains(value);
            }
            catch (Exception)
            {
                retVal = false;
            }
            m_Mutex.ReleaseMutex();
            return retVal;
        }

        public void Ban(string value)
        {
            m_Mutex.WaitOne();
            try
            {
                m_Names.Add(value);
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
