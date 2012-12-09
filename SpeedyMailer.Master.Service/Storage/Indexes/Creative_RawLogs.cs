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
            public List<ReducedMailLogEntry> RawLogs { get; set; }
            public string DroneId { get; set; }
        }
        public Creative_RawLogs()
        {
            Map = snapshots => snapshots
                .Select(x => new { DroneId = x.Drone.Id, x.RawLogs });

            Reduce = result => result
                .GroupBy(x => x.DroneId )
                .Select(x => new { DroneId = x.Key, RawLogs = x.SelectMany(m => m.RawLogs).ToList() });
        }
    }
}
