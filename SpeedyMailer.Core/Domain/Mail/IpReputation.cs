using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class IpReputation
	{
		[BsonId(IdGenerator = typeof(TypeNameIdGenerator))]
		public virtual string Id { get; set; }

		public IDictionary<string, List<DateTime>> BlockingHistory { get; set; }
		public IDictionary<string, List<DateTime>> ResumingHistory { get; set; }
	}
}