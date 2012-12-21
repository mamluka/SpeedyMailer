using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
    public class Creative_BouncedEmails : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_BouncedEmails.ReduceResult>
    {
        public class ReduceResult
        {
            public string CreativeId { get; set; }
            public List<GenericMailEvent> Bounced { get; set; }
        }
        public Creative_BouncedEmails()
        {
            Map = snapshots => snapshots.SelectMany(x => x.MailBounced.Select(m => new { m.CreativeId, m.DomainGroup, m.Recipient, m.Time }), (snapshot, p) => new
                {
                    CreativeId = p.CreativeId,
                    Bounced = new[] { p }
                });

            Reduce = result => result
                                   .GroupBy(x => x.CreativeId)
                                   .Select(
                                       x => new
                                           {
                                               CreativeId = x.Key,
                                               Bounced = x.SelectMany(m => m.Bounced).Select(m => new { DomainGroup = m.DomainGroup, Recipient = m.Recipient, Time = m.Time , CreativeId = m.CreativeId }),
                                           });
        }
    }
}
