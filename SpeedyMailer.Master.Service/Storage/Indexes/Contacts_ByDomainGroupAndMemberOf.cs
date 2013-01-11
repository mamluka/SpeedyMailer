using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Contacts_ByDomainGroupAndMemberOf : AbstractIndexCreationTask<Contact>
	{
		public Contacts_ByDomainGroupAndMemberOf()
		{
			Map = contact => contact.Select(x => new { x.DomainGroup, x.MemberOf });
		}
	}
}
