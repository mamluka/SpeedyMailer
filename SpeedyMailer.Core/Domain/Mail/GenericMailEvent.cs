using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
    public class GenericMailEvent
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public virtual string Id { get; set; }

        public string Recipient { get; set; }
        public DateTime Time { get; set; }
        public string DomainGroup { get; set; }
	    public string CreativeId { get; set; }
        public int Type { get; set; }
    }
}