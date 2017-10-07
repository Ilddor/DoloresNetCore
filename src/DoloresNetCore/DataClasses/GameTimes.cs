using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class GameTimes : IState
    {
        public Dictionary<ulong, Dictionary<string, long>> m_Times = new Dictionary<ulong, Dictionary<string, long>>();
        public Dictionary<ulong, Tuple<string, DateTime>> m_StartTimes = new Dictionary<ulong, Tuple<string, DateTime>>();
        public Mutex m_Mutex = new Mutex();

        public void Save()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("gameTimes.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_Times));
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_StartTimes));
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
                using (Stream stream = File.Open("gameTimes.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    m_Times = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, long>>>(streamReader.ReadLine());
                    m_StartTimes = JsonConvert.DeserializeObject<Dictionary<ulong, Tuple<string, DateTime>>>(streamReader.ReadLine());
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
