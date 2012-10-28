using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Contacts_ByMemberOf:AbstractIndexCreationTask<Contact>
	{
		public Contacts_ByMemberOf()
		{
//			Map = contacts => contacts.Where(x=> x.MemberOf.Contains())
		}
	}
}
