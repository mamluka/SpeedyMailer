using System.Collections.Generic;
using SpeedyMailer.Core.Rules;

namespace SpeedyMailer.Core.Domain.Contacts
{
	public class Contact
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string Phone { get; set; }
		public string Ip { get; set; }
		public List<string> MemberOf { get; set; }
		public string DomainGroup { get; set; }

		public Contact()
		{
			MemberOf = new List<string>();
		}
	}
}