using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class LogsStore : RecordManager<MailLogEntry>,ICycleSocket
	{
		public LogsStore(DroneSettings droneSettings)
			: base(droneSettings.StoreHostname, "logs")
		{ }

		public IList<MailLogEntry> GetAllLogs()
		{
			return Find(Query.EQ(PropertyName(x => x.Level), "INFO")).ToList();
		}

		public void CycleSocket()
		{
			Count();
		}
	}
}