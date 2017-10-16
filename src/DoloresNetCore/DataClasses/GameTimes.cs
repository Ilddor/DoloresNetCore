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

        public IEnumerable<KeyValuePair<string, long>> GetTopGames(int numTopResults, IEnumerable<ulong> userSet = null)
        {
            IEnumerable<KeyValuePair<string, long>> list = null;

            m_Mutex.WaitOne();
            try
            {

                Dictionary<ulong, Dictionary<string, long>> tmp = m_Times;
                if(userSet != null)
                    tmp = m_Times.Where(x => userSet.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                list = tmp.Values
                    .SelectMany(x => x)
                    .GroupBy(x => x.Key)
                    .Select(g => new KeyValuePair<string, long>(g.Key, g.Sum(x => x.Value)))
                        .OrderByDescending(x => x.Value)
                        .Take(numTopResults);
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();

            return list;
        }

        public IEnumerable<KeyValuePair<ulong, long>> GetTopUsers(int numTopResults)
        {
            IEnumerable<KeyValuePair<ulong, long>> list = null;

            m_Mutex.WaitOne();
            try
            {
                list = m_Times.GroupBy(x => x.Key)
                    .Select(g => new KeyValuePair<ulong, long>(
                        g.Key,
                        g.Sum(x => x.Value.Sum(y => y.Value))))
                        .OrderByDescending(x => x.Value)
                        .Take(numTopResults);

            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();

            return list;
        }

        public IEnumerable<ulong> GetAllUsers()
        {
            IEnumerable<ulong> list = null;

            m_Mutex.WaitOne();
            try
            {
                list = m_Times.Keys;
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();

            return list;
        }
    }
}
