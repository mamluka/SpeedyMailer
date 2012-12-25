using System;
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
			public ClickReduceResult[] ClickedBy { get; set; }

			public class ClickReduceResult
			{
				public string Domain { get; set; }
				public DateTime Time { get; set; }
				public string Contactid { get; set; }
			}

		}
		public Creative_ClickActions()
		{
			Map = snapshots => snapshots
								   .SelectMany(x => x.ClickActions, (snapshot, x) => new
																						 {
																							 ClickedBy = new[] { new ReduceResult.ClickReduceResult { Contactid = x.ContactId, Time = x.Date, Domain = x.Domain } },
																							 x.CreativeId
																						 });

			Reduce = result => result
								   .GroupBy(x => x.CreativeId)
								   .Select(x => new { ClickedBy = x.SelectMany(m => m.ClickedBy).ToArray(), CreativeId = x.Key });
		}
	}
}
