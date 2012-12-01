using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Emails
{
	public class ClickAction
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		
		public string ContactId { get; set; }
		public string CreativeId { get; set; }
		public DateTime Date { get; set; }
	}
}