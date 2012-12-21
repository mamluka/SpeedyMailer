using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Creative_RawLogs : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_RawLogs.ReduceResult>
	{
		public class ReduceResult
		{
			public string Log { get; set; }
		}

		public Creative_RawLogs()
		{
			Map = snapshots => snapshots
				.SelectMany(x => x.RawLogs, (x, m) => new { Log = m });

			Reduce = result =>
					 result.Select(x => new { Log = x.Log });
		}
	}
}
