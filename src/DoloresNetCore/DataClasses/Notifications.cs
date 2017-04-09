using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolores.DataClasses
{
    public class Notifications
    {
        private Dictionary<ulong, ulong> m_Notifications = new Dictionary<ulong, ulong>();
        private Mutex m_Mutex = new Mutex();

        public void AddNotification(ulong user, ulong notification)
        {
            m_Mutex.WaitOne();
            try
            {
                if (!m_Notifications.ContainsKey(notification))
                {
                    m_Notifications.Add(notification, user);
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
        }

        public ulong ShouldNofity(ulong notification)
        {
            ulong result = 0;
            m_Mutex.WaitOne();
            try
            {
                if (m_Notifications.ContainsKey(notification))
                {
                    result = m_Notifications[notification];
                    m_Notifications.Remove(notification);
                }
            }
            catch (Exception) { }
            m_Mutex.ReleaseMutex();
            return result;
        }

        public void SaveToFile()
        {
            m_Mutex.WaitOne();
            try
            {
                using (FileStream stream = File.Open("notifications.dat", FileMode.Create))
                {
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(JsonConvert.SerializeObject(m_Notifications));
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
                using (Stream stream = File.Open("notifications.dat", FileMode.Open))
                {
                    var streamReader = new StreamReader(stream);
                    m_Notifications = JsonConvert.DeserializeObject<Dictionary<ulong, ulong>>(streamReader.ReadLine());
                }
            }
            catch (IOException) { }
            m_Mutex.ReleaseMutex();
        }
    }
}
