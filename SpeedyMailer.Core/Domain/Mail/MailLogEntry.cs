using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Mongol;

namespace SpeedyMailer.Core.Domain.Mail
{
	[CollectionName("log")]
	public class MailLogEntry
	{
		[BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string msg { get; set; }
		public string level { get; set; }
		public DateTime time { get; set; }
	}
}