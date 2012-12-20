using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Mongol;
using NLog;
using NLog.Targets;

namespace SpeedyMailer.Core.Logging
{
	[Target("Mongo")]
	public sealed class MongoTarget : TargetWithLayout
	{
		public MongoTarget()
		{
			Host = "mongodb://localhost:27027/?safe=true";
		}

		public string Host { get; set; }

		protected override void Write(LogEventInfo logEvent)
		{
			string logMessage = Layout.Render(logEvent);

			WriteToMongo(logMessage);
		}

		private void WriteToMongo(string message)
		{
			var logStore = new RecordManager<DroneException>(Host);
			logStore.BatchInsert(new[] { new DroneException { LogMessage = message } });
		}
	}

	public class DroneException
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string LogMessage { get; set; }
	}
}
