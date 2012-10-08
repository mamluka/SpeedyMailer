using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Mongol;

namespace SpeedyMailer.Core.Domain.Creative
{
	public class CreativePackage
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string Body { get; set; }
		public string Subject { get; set; }
		public string To { get; set; }
		public int Interval { get; set; }
	}
}