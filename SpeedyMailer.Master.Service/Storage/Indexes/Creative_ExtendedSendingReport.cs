using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
    public class Creative_ExtendedSendingReport : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_ExtendedSendingReport.ReduceResult>
    {
        public class ReduceResult
        {
            public string CreativeId { get; set; }
            public List<GenericMailEvent> Sends { get; set; }
            public List<GenericMailEvent> Bounces { get; set; }
            public List<GenericMailEvent> Defers { get; set; }
        }
        public Creative_ExtendedSendingReport()
        {
            Map = snapshots => snapshots
                                   .Select(x => new { CreativeId = x.CurrentCreativeId, Sends = x.MailSent, Bounces = x.MailBounced, Defers = x.MailDeferred });

            Reduce = result => result
                                   .GroupBy(x => x.CreativeId)
                                   .Select(
                                       x =>
                                       new
                                           {
                                               CreativeId = x.Key,
                                               Sends = x.SelectMany(m => m.Sends).Select(m => new GenericMailEvent { DomainGroup = m.DomainGroup, Recipient = m.Recipient, Time = m.Time }),
                                               Bounces = x.SelectMany(m => m.Bounces).Select(m => new GenericMailEvent { DomainGroup = m.DomainGroup, Recipient = m.Recipient, Time = m.Time }),
                                               Defers = x.SelectMany(m => m.Defers).Select(m => new GenericMailEvent { DomainGroup = m.DomainGroup, Recipient = m.Recipient, Time = m.Time })
                                           });
        }
    }
}
