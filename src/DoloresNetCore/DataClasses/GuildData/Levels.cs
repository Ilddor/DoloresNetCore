using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dolores.DataClasses.GuildData
{
    public class Levels
    {
		[JsonProperty("Experience")]
		private Dictionary<ulong, ulong> m_Experience = new Dictionary<ulong, ulong>();
		private Mutex m_Mutex = new Mutex();

		public void AddExperience(ulong user, ulong experience)
		{
			m_Mutex.WaitOne();
			try
			{
				if (!m_Experience.ContainsKey(user))
				{
					m_Experience.Add(user, 0);
				}

				m_Experience[user] += experience;
			}
			catch (Exception) { }
			m_Mutex.ReleaseMutex();
		}

		public IEnumerable<KeyValuePair<ulong, ulong>> GetTopUsers(int count)
		{
			IEnumerable<KeyValuePair<ulong, ulong>> result = new List<KeyValuePair<ulong, ulong>>(); ;
			m_Mutex.WaitOne();
			try
			{
				result = m_Experience.ToList().OrderByDescending(x => x.Value).Take(count);
			}
			catch (Exception) { }
			m_Mutex.ReleaseMutex();

			return result;
		}
	}
}
