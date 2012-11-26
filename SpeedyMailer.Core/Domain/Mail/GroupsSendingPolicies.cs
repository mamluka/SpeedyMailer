using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class GroupsSendingPolicies
	{
		[BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
		public virtual string Id { get; set; }

		public IDictionary<string,GroupSendingPolicy> GroupSendingPolicies { get; set; }
	}

	public class GroupSendingPolicy
	{
		public DateTime ResumeAt { get; set; }
	}
}