using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
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
		    public int TotalDefers { get; set; }
		}
		public Creative_SendingReport()
		{
		    Map = snapshots => snapshots
		                           .Select( x => new {CreativeId = x.CurrentCreativeId, TotalSends = x.MailSent.Count, TotalBounces = x.MailBounced.Count, TotalDefers = x.MailDeferred.Count});

		    Reduce = result => result
		                           .GroupBy(x => x.CreativeId)
		                           .Select(
		                               x =>
		                               new
		                                   {
		                                       CreativeId = x.Key,
		                                       TotalSends = x.Sum(total => total.TotalSends),
		                                       TotalBounces = x.Sum(total => total.TotalBounces),
		                                       TotalDefers = x.Sum(total => total.TotalDefers)
		                                   });
		}
	}
}
