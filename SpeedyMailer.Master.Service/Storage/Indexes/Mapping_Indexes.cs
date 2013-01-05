using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;

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

	public class Tasks_ByDateAndStatus : AbstractIndexCreationTask<PersistentTask>
	{
		public Tasks_ByDateAndStatus()
		{
			Map = persistentTasks => persistentTasks.Select(x => new { x.CreateDate, x.Status });
		}
	}

	public class Templates_ByType : AbstractIndexCreationTask<Template>
	{
		public Templates_ByType()
		{
			Map = templates => templates.Select(x => new { x.Type });
		}
	}


}