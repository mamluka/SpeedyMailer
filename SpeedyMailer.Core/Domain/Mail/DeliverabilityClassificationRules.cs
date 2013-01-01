using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class DeliverabilityClassificationRules
	{
		[BsonId(IdGenerator = typeof(TypeNameIdGenerator))]
		public virtual string Id { get; set; }

		public List<HeuristicRule> Rules { get; set; }
	}

	public class TypeNameIdGenerator : IIdGenerator
	{
		public object GenerateId(object container, object document)
		{
			return document.GetType().FullName;
		}

		public bool IsEmpty(object id)
		{
			return string.IsNullOrEmpty((string)id);
		}
	}

	public class HeuristicRule
	{
		public string Condition { get; set; }
		public Classification Type { get; set; }
		public dynamic Data { get; set; }
	}
}