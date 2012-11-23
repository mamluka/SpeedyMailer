using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Builders;
using Mongol;
using NLog;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class LogsStore : RecordManager<MailLogEntry>
	{
		private readonly OmniRecordManager _omniRecordManager;
		private readonly Logger _logger;

		public LogsStore(DroneSettings droneSettings, OmniRecordManager omniRecordManager, Logger logger)
			: base(droneSettings.StoreHostname, "log")
		{
			_logger = logger;
			_omniRecordManager = omniRecordManager;
		}

		public IList<MailLogEntry> GetUnprocessedLogs()
		{
			var last = _omniRecordManager.GetSingle<LastProcessedLog>();

			if (last == null)
				return Find(Query.EQ("level", "INFO"), SortBy.Ascending("time"))
					.ToList();

			_logger.Info("When loading postfix logs, the last processed log was from: {0}", last.Time.ToLongTimeString());

			return Find(Query.EQ("level", "INFO").And(Query.GT("time", last.Time)), SortBy.Ascending("time"))
			.ToList();
		}

		public void MarkProcessed(IList<MailLogEntry> mailLogEntries)
		{
			if (!mailLogEntries.Any())
				return;

			var last = _omniRecordManager.GetSingle<LastProcessedLog>() ?? new LastProcessedLog();

			last.Time = mailLogEntries
				.OrderByDescending(x => x.time).First().time;

			_logger.Info("After processing postfix logs the last log is from: {0}", last.Time.ToLongTimeString());

			_omniRecordManager.UpdateOrInsert(last);
		}
	}

	public class LastProcessedLog
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public DateTime Time { get; set; }
	}
}