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
            public string CreativeId { get; set; }
            public List<ReducedMailLogEntry> RawLogs { get; set; }
        }
        public Creative_RawLogs()
        {
            Map = snapshots => snapshots
                .Select(x => new { CreativeId = x.CurrentCreativeId, x.RawLogs });

            Reduce = result => result
                .GroupBy(x => x.CreativeId)
                .Select(x => new { CreativeId = x.Key, RawLogs = x.SelectMany(m => m.RawLogs).ToList() });
        }
    }
}
