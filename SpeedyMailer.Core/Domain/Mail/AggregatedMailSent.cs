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

	public class MailSent : IHasDomainGroup, IHasRecipient
	{
		protected bool Equals(MailSent other)
		{
			return string.Equals(Recipient, other.Recipient) && string.Equals(CreativeId, other.CreativeId);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MailSent)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Recipient != null ? Recipient.GetHashCode() : 0) * 397) ^ (CreativeId != null ? CreativeId.GetHashCode() : 0);
			}
		}

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string Recipient { get; set; }
		public DateTime Time { get; set; }
		public string DomainGroup { get; set; }
		public string CreativeId { get; set; }

		public string Domain { get; set; }
	}

	public class MailBounced : IHasDomainGroup, IHasRecipient, IHasRelayMessage, IHasTime, IHasCreativeId
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string Recipient { get; set; }
		public DateTime Time { get; set; }
		public string DomainGroup { get; set; }
		public string Message { get; set; }

		public string CreativeId { get; set; }

		public string Domain { get; set; }

		public MailClassfication Classification { get; set; }
	}

	public class MailDeferred : IHasDomainGroup, IHasRecipient, IHasRelayMessage, IHasTime, IHasCreativeId
	{
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public string Recipient { get; set; }
		public DateTime Time { get; set; }
		public string DomainGroup { get; set; }
		public string Message { get; set; }
		public string CreativeId { get; set; }

		public string Domain { get; set; }

		public MailClassfication Classification { get; set; }
	}

	public interface IHasDomainGroup
	{
		string DomainGroup { get; set; }
	}

	public interface IHasRecipient
	{
		string Recipient { get; set; }
	}

	public interface IHasRelayMessage
	{
		string Message { get; set; }
	}

	public interface IHasTime
	{
		DateTime Time { get; set; }
	}

	public interface IHasCreativeId
	{
		string CreativeId { get; set; }
	}

	public class AggregatedMailEvents<T>
	{
		public IList<T> MailEvents { get; set; }
	}


}