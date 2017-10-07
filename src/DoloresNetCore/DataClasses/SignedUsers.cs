using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class SignedUsers : IState
    {
        public Dictionary<ulong, bool> m_Users = new Dictionary<ulong, bool>();
        public Mutex m_Mutex = new Mutex();

        public void Save()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("signedUsers.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    foreach (var it in m_Users)
                    {
                        streamWriter.WriteLine(it.Key);
                    }
                    streamWriter.Flush();
                    stream.Flush();
                }
            }
            catch (IOException) { }
            m_Mutex.ReleaseMutex();
        }

        public void Load()
        {
            m_Mutex.WaitOne();
            try
            {
                using (Stream stream = File.Open("signedUsers.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    while (!streamReader.EndOfStream)
                    {
                        m_Users.Add(ulong.Parse(streamReader.ReadLine()), true);
                    }
                }
            }
            catch (IOException) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
