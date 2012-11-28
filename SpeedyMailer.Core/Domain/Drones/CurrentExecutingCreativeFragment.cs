using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Master;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class CurrentExecutingCreativeFragment
	{
		public CreativeFragment CreativeFragment { get; set; }

		[BsonId(IdGenerator = typeof(ContactByClassIdGenerator))]
		public virtual string Id { get; set; }



	}

	public class ContactByClassIdGenerator : IIdGenerator
	{
		public object GenerateId(object container, object document)
		{
			return "_" + document.GetType().FullName;
		}

		public bool IsEmpty(object id)
		{
			return string.IsNullOrEmpty((string)id);
		}
	}
}