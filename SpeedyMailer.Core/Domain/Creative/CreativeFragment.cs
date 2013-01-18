using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Master;

namespace SpeedyMailer.Core.Domain.Creative
{
	public class CreativeFragment
	{
		public string Id { get; set; }
		public string UnsubscribeTemplate { get; set; }
		public List<Recipient> Recipients { get; set; }
		public string HtmlBody { get; set; }
		public string Subject { get; set; }
		public Service Service { get; set; }
		public string CreativeId { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FragmentStatus Status { get; set; }

		public string FromName { get; set; }
		public string FromAddressDomainPrefix { get; set; }
		public string DealUrl { get; set; }

		public string TextBody { get; set; }

		public string FetchedBy { get; set; }

		public DateTime FetchedAt { get; set; }
	}

	public class Recipient
	{
		public string ContactId { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public int Interval { get; set; }
		public string Group { get; set; }
	}
}