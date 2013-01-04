using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Creative_UnclassifiedEmails : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_UnclassifiedEmails.ReduceResult>
	{
		public class ReduceResult
		{
			public string CreativeId { get; set; }
			public List<GenericMailEvent> Unclassified { get; set; }
		}
		public Creative_UnclassifiedEmails()
		{
			Map = snapshots => snapshots.SelectMany(x => x.Unclassified.Select(m => new { m.CreativeId, m.DomainGroup, m.Recipient, m.Time, m.Message }), (snapshot, p) => new
				{
					p.CreativeId,
					Unclassified = new[] { p }
				});

			Reduce = result => result
								   .GroupBy(x => x.CreativeId)
								   .Select(
									   x => new
										   {
											   CreativeId = x.Key,
											   Unclassified = x.SelectMany(m => m.Unclassified).Select(m => new {m.DomainGroup, m.Recipient, m.Time, m.CreativeId, m.Message }),
										   });
		}
	}
}
