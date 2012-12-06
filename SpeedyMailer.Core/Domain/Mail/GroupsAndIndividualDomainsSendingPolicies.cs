using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class GroupsAndIndividualDomainsSendingPolicies
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public IDictionary<string,ResumeSendingPolicy> GroupSendingPolicies { get; set; }
	}

	public class ResumeSendingPolicy
	{
		public DateTime ResumeAt { get; set; }
	}
}