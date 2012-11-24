using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class UnDeliveredMailClassificationHeuristicsRules
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public List<HeuristicRule> HardBounceRules { get; set; }
		public List<HeuristicRule> IpBlockingRules { get; set; }
	}

	public class HeuristicRule
	{
		public string Condition { get; set; }
		public TimeSpan TimeSpan { get; set; }
	}
}