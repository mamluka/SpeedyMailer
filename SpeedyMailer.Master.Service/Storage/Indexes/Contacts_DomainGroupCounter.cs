using System.Collections.Generic;
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
			public string ListId { get; set; }
		}
		public Contacts_DomainGroupCounter()
		{
			Map = contacts => contacts
				.SelectMany(x => x.MemberOf.Select(m => new { __document_id = x.Id, x.DomainGroup, ListId = m }))
				.Select(x => new { x.DomainGroup, Count = 1, x.ListId });

			Reduce = results => results
				.GroupBy(x => new { x.ListId, x.DomainGroup })
				.Select(x => new {x.Key.DomainGroup, Count = x.Sum(m => m.Count), x.Key.ListId});
		}
	}
}
