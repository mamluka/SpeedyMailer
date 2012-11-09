using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class AggregatedMailSent : AggregatedMailEvents<MailSent>
	{

	}

	public class AggregatedMailDeferred : AggregatedMailEvents<MailDeferred>
	{
	}

	public class AggregatedMailBounced : AggregatedMailEvents<MailBounced>
	{
	}

	public interface IHasDomainGroup
	{
		string DomainGroup { get; set; }
	}

	public class MailSent:IHasDomainGroup
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }
		
		public string Recipient { get; set; }
		public DateTime Time { get; set; }
		public string DomainGroup { get; set; }
	}

	public class MailBounced : IHasDomainGroup
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }
		
		public string Recipient { get; set; }
		public DateTime Time { get; set; }
		public string DomainGroup { get; set; }
		public string Message { get; set; }
	}

	public class MailDeferred : IHasDomainGroup
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string Recipient { get; set; }
		public DateTime Time { get; set; }
		public string DomainGroup { get; set; }
		public string Message { get; set; }
	}

	public class AggregatedMailEvents<T>
	{
		public IList<T> MailEvents { get; set; }
	}
}