using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Creative_SendingReport : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_SendingReport.ReduceResult>
	{
		public class ReduceResult
		{
			public string CreativeId { get; set; }
			public int TotalSends { get; set; }
			public int TotalBounces { get; set; }
		}
		public Creative_SendingReport()
		{
			Map = snapshots => snapshots.SelectMany(x => x.MailSent.Select(m => new { m.CreativeId, TotalSends = 1, TotalBounces = 0 })
				.Concat(x.MailBounced.Select(m => new { m.CreativeId, TotalSends = 0, TotalBounces = 1 })), (snapshot, m) => new
					{
						m.CreativeId,
						m.TotalSends,
						m.TotalBounces,
					});

			Reduce = result => result
								   .GroupBy(x => x.CreativeId)
								   .Select(
									   x =>
									   new
										   {
											   CreativeId = x.Key,
											   TotalSends = x.Sum(total => total.TotalSends),
											   TotalBounces = x.Sum(total => total.TotalBounces),
										   });
		}
	}
}
