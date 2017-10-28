using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

namespace Dolores.DataClasses
{
    public class BannedSubreddits
    {
        [JsonProperty("Names")]
        private HashSet<string> m_Names = new HashSet<string>();
        private Mutex m_Mutex = new Mutex();

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
