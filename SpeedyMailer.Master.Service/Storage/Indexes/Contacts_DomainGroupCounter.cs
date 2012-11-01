using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Contacts_DomainGroupCounter : AbstractIndexCreationTask<Contact, Contacts_DomainGroupCounter.ReduceResult>
	{
		public class ReduceResult
		{
			public string DomainGroup { get; set; }
			public int Count { get; set; }
		}
		public Contacts_DomainGroupCounter()
		{
			Map = contacts => contacts.Select(x => new { x.DomainGroup, Count = 1 });
			Reduce = results => results.GroupBy(x => x.DomainGroup).Select(x => new { DomainGroup =  x.Key, Count = x.Sum(m => m.Count) });
		}
	}
}
