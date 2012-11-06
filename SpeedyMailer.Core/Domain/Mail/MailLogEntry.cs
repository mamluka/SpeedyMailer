using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Mongol;

namespace SpeedyMailer.Core.Domain.Mail
{
	[CollectionName("log")]
	public class MailLogEntry
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string Msg { get; set; }
		public string Level { get; set; }
		public DateTime Time { get; set; }
	}
}