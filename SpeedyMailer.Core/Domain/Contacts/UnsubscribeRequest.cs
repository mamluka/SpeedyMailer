using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Contacts
{
	public class UnsubscribeRequest
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string CreativeId { get; set; }
		public string ContactId { get; set; }
		public DateTime Date { get; set; }
	}
}