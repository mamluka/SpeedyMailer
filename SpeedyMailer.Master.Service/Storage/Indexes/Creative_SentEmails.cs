using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
    public class Creative_SentEmails : AbstractIndexCreationTask<DroneStateSnapshoot, Creative_SentEmails.ReduceResult>
    {
        public class ReduceResult
        {
            public string CreativeId { get; set; }
            public List<GenericMailEvent> Sends { get; set; }
        }
        public Creative_SentEmails()
        {
            Map = snapshots => snapshots.SelectMany(x => x.MailSent.Select(m => new { m.CreativeId, m.DomainGroup, m.Recipient, m.Time }), (snapshot, p) => new
                {
	                p.CreativeId,
                    Sends = new[] { p }
                });

            Reduce = result => result
                                   .GroupBy(x => x.CreativeId)
                                   .Select(
                                       x => new
                                           {
                                               CreativeId = x.Key,
											   Sends = x.SelectMany(m => m.Sends).Select(m => new {m.DomainGroup, m.Recipient, m.Time, m.CreativeId }),
                                           });
        }
    }
}
