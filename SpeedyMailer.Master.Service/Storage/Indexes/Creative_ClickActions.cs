using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Creative_ClickActions : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_ClickActions.ReduceResult>
	{
		public class ReduceResult
		{
			public string CreativeId { get; set; }
			public string[] ClickedBy { get; set; }
		}
		public Creative_ClickActions()
		{
			Map = snapshots => snapshots
				                   .SelectMany(x => x.ClickActions, (snapshot, x) => new
					                                                                     {
						                                                                     ClickedBy = new[] {x.ContactId},
						                                                                     x.CreativeId
					                                                                     });

			Reduce = result => result
								   .GroupBy(x => x.CreativeId)
								   .Select(x => new { ClickedBy = x.SelectMany(m => m.ClickedBy).ToArray(), CreativeId = x.Key });
		}
	}
}
