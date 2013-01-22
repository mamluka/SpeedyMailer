using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Creative_AllBounces : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_AllBounces.ReduceResult>
	{
		public class ReduceResult
		{
			public string Group { get; set; }
			public List<GenericMailEvent> Bounced { get; set; }
		}
		public Creative_AllBounces()
		{
			Map = snapshots => snapshots.SelectMany(x => x.MailBounced.Select(m => new { m.CreativeId, m.DomainGroup, m.Recipient, m.Time, m.Message }), (snapshot, p) => new
				{
					Group = "All",
					Bounced = new[] { p }
				});

			Reduce = result => result
								   .GroupBy(x => x.Group)
								   .Select(
									   x => new
										   {
											   Group = x.Key,
											   Bounced = x.SelectMany(m => m.Bounced).Select(m => new { DomainGroup = m.DomainGroup, Recipient = m.Recipient, Time = m.Time, CreativeId = m.CreativeId, Message = m.Message }),
										   });
		}
	}
}
