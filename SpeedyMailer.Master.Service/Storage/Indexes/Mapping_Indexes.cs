using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Contacts_ByMemberOf : AbstractIndexCreationTask<Contact>
	{
		public Contacts_ByMemberOf()
		{
			Map = contacts => contacts.Select(x => new { x.MemberOf });
		}
	}

	public class Fragments_ByStatus : AbstractIndexCreationTask<CreativeFragment>
	{
		public Fragments_ByStatus()
		{
			Map = creativeFragments => creativeFragments.Select(x => new { x.Status });
		}
	}


}