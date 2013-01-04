using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using SpeedyMailer.Core.Utilities.Extentions;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class MailBounced : IHasDomainGroup, IHasRecipient, IHasRelayMessage, IHasTime, IHasCreativeId, IHasClassification,IHasDomain
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

	public static class MailBouncedExtentions
	{
		public static IList<string> GetDomains(this IEnumerable<MailBounced> target, Classification classificatioType)
		{
			return target
				.Where(x => x.Classification.Type == classificatioType)
				.Where(x => x.Domain.HasValue())
				.Select(x => x.Domain)
				.ToList();
		}
	}
}