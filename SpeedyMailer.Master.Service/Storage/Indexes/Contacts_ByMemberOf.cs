using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Contacts_ByMemberOf : AbstractIndexCreationTask<Contact>
	{
		public Contacts_ByMemberOf()
		{
			Map = contacts => contacts.Select(x => new { x.MemberOf });
		}
	}
}