using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class LogsStore : RecordManager<MailLogEntry>
	{
		private OmniRecordManager _omniRecordManager;

		public LogsStore(DroneSettings droneSettings, OmniRecordManager omniRecordManager)
			: base(droneSettings.StoreHostname, "logs")
		{
			_omniRecordManager = omniRecordManager;
		}

		public IList<MailLogEntry> GetUnprocessedLogs()
		{
			var last = _omniRecordManager.GetSingle<LastProcessedLog>();

			if (last == null)
				return Find(Query.EQ("level", "INFO"), SortBy.Ascending("time"))
				.ToList();

			return Find(Query.EQ("level", "INFO").And(Query.GT("time", last.Time)), SortBy.Ascending("time"))
			.ToList();
		}

		public void MarkProcessed(IList<MailLogEntry> mailLogEntries)
		{
			var last = _omniRecordManager.GetSingle<LastProcessedLog>() ?? new LastProcessedLog();

			var lastTime=mailLogEntries
				.OrderByDescending(x => x.time).First();

		}
	}

	public class LastProcessedLog
	{
		public DateTime Time { get; set; }
	}
}