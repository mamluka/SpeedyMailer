using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
    public class Creative_DeferredEmails : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_DeferredEmails.ReduceResult>
    {
        public class ReduceResult
        {
            public string CreativeId { get; set; }
            public List<GenericMailEvent> Deferred { get; set; }
        }
        public Creative_DeferredEmails()
        {
            Map = snapshots => snapshots.SelectMany(x => x.MailDeferred.Select(m => new { m.CreativeId, m.DomainGroup, m.Recipient, m.Time }), (snapshot, p) => new
                {
                    CreativeId = p.CreativeId,
                    Deferred = new[] { p }
                });

            Reduce = result => result
                                   .GroupBy(x => x.CreativeId)
                                   .Select(
                                       x => new
                                           {
                                               CreativeId = x.Key,
											   Deferred = x.SelectMany(m => m.Deferred).Select(m => new { DomainGroup = m.DomainGroup, Recipient = m.Recipient, Time = m.Time, CreativeId = m.CreativeId }),
                                           });
        }
    }
}
