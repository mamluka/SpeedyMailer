using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class IpBlockingGroups
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public List<string> Groups { get; set; }
	}
}