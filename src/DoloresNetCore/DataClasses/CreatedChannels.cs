using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class CreatedChannels
    {
        public Dictionary<ulong, bool> m_Channels = new Dictionary<ulong, bool>();
        public Mutex m_Mutex = new Mutex();
        
        public void SaveToFile()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("channels.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    foreach (var it in m_Channels)
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

        public void LoadFromFile()
        {
            m_Mutex.WaitOne();
            try
            {
                using (Stream stream = File.Open("channels.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    while (!streamReader.EndOfStream)
                    {
                        m_Channels.Add(ulong.Parse(streamReader.ReadLine()), true);
                    }
                }
            }
            catch (IOException) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
