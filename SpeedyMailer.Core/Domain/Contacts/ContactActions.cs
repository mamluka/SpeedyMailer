using System;
using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Contacts
{
	public class ContactActions
	{
		public string Id { get; set; }
		public string ContactId { get; set; }
		public List<string> Clicks { get; set; }
		public List<string> Opens { get; set; }

		public DateTime Date { get; set; }
	}
}