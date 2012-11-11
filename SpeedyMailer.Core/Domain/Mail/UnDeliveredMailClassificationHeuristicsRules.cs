using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class UnDeliveredMailClassificationHeuristicsRules
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public List<string> HardBounceRules { get; set; }
		public List<string> IpBlockingRules { get; set; }
	}
}