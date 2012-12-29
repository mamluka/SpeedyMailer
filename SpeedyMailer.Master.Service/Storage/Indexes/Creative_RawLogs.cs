using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
//	public class Creative_RawLogs : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_RawLogs.ReduceResult>
//	{
//		public class ReduceResult
//		{
//			public List<string> Logs { get; set; }
//			public string Group { get; set; }
//		}
//
//		public Creative_RawLogs()
//		{
//			Map = snapshots => snapshots
//				.Select(x => new { Logs = x.RawLogs, Group = "All" });
//
//			Reduce = result => result
//								   .GroupBy(x => x.Group == "All")
//								   .Select(x => new { Logs = x.SelectMany(r => r.Logs, (p, m) => m).ToList(), Group = "All" });
//
//		}
//	}
}
