using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Creative_UnsubscribeRequests : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_UnsubscribeRequests.ReduceResult>
	{
		public class ReduceResult
		{
			public string CreativeId { get; set; }
			public string[] UnsubscribeRequests { get; set; }
		}
		public Creative_UnsubscribeRequests()
		{
			Map = snapshots => snapshots
				                   .SelectMany(x => x.UnsubscribeRequests, (snapshot, x) => new
					                                                                     {
																							 UnsubscribeRequests = new[] { x.ContactId },
						                                                                     x.CreativeId
					                                                                     });

			Reduce = result => result
								   .GroupBy(x => x.CreativeId)
								   .Select(x => new { UnsubscribeRequests = x.SelectMany(m => m.UnsubscribeRequests).ToArray(), CreativeId = x.Key });
		}
	}
}
