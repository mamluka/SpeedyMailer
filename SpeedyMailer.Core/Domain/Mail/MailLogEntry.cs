using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Mongol;

namespace SpeedyMailer.Core.Domain.Mail
{
	[CollectionName("log")]
	public class MailLogEntry
	{
		[BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
		public virtual BsonObjectId Id { get; set; }

		public string msg { get; set; }
		public string level { get; set; }
		public DateTime time { get; set; }
		public string sys { get; set; }
		public DateTime time_rcvd { get; set; }
		public int syslog_fac { get; set; }
		public int syslog_sever { get; set; }
		public string syslog_tag { get; set; }
		public string procid { get; set; }
		public string pid { get; set; }
	}
}