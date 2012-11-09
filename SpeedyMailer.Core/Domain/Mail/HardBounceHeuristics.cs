using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class HardBounceHeuristics
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public IList<string> HardBounceRules { get; set; }
	}
}