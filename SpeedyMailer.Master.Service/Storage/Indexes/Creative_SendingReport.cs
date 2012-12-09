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
                                   .SelectMany(x => x.MailSent.GroupBy(m => m.CreativeId).Select(m => new { CreativeId = m.Key, Type = "Sent", Total = m.Count() }))
                                   .Concat(snapshots.SelectMany(x => x.MailBounced.GroupBy(m => m.CreativeId).Select(m => new { CreativeId = m.Key, Type = "Bounced", Total = m.Count() })))
                                   .Concat(snapshots.SelectMany(x => x.MailDeferred.GroupBy(m => m.CreativeId).Select(m => new { CreativeId = m.Key, Type = "Deferred", Total = m.Count() })))
                                   .GroupBy(x => x.CreativeId)
                                   .Select(x => new { CreativeId = x.Key, TotalSends = x.Count(m => m.Type == "Sent"), TotalBounces = x.Count(m => m.Type == "Bounced"), TotalDefers = x.Count(m => m.Type == "Deferred") });

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
