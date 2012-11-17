using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class LogsStore : RecordManager<MailLogEntry>
	{
		public LogsStore(DroneSettings droneSettings)
			: base(droneSettings.StoreHostname, "logs")
		{ }

		public IList<MailLogEntry> GetUnprocessedLogs()
		{
			return Find(Query.EQ("level", "INFO"))
				.ToList();
		}
	}
}