using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Rules
{
	public class IntervalRule
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public List<string> Conditons { get; set; }
		public int Interval { get; set; }

		public string Group { get; set; }
	}
}