using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
    public class RawLogsIndexTests : IntegrationTestBase
    {
        [Test]
        public void Index_WhenGivenSnapShots_ShouldMapReduceAllRawLogs()
        {
            var snapshots = new[]
                {
                    new DroneStateSnapshoot
                        {
                            Drone = new Drone { Id = "drone1"},
                            RawLogs = new List<ReducedMailLogEntry>
                                {
                                    new ReducedMailLogEntry {Level = "info", Message = "message 1", Time = DateTime.UtcNow},
                                    new ReducedMailLogEntry {Level = "info", Message = "message 2", Time = DateTime.UtcNow}
                                }
                        },
                    new DroneStateSnapshoot
                        {
                            Drone = new Drone { Id = "drone1"},
                            RawLogs = new List<ReducedMailLogEntry>
                                {
                                    new ReducedMailLogEntry {Level = "info", Message = "message 3", Time = DateTime.UtcNow},
                                    new ReducedMailLogEntry {Level = "info", Message = "message 4", Time = DateTime.UtcNow},
                                    new ReducedMailLogEntry {Level = "info", Message = "message 5", Time = DateTime.UtcNow}
                                }
                        },
            new DroneStateSnapshoot
                        {
                            Drone = new Drone { Id = "drone2"},
                            RawLogs = new List<ReducedMailLogEntry>
                                {
                                    new ReducedMailLogEntry {Level = "info", Message = "message 3", Time = DateTime.UtcNow},
                                    new ReducedMailLogEntry {Level = "info", Message = "message 4", Time = DateTime.UtcNow},
                                    new ReducedMailLogEntry {Level = "info", Message = "message 5", Time = DateTime.UtcNow}
                                }
                        }
                }.ToList();

            snapshots.ForEach(Store.Store);

            Store.WaitForIndexNotToBeStale<Creative_RawLogs.ReduceResult, Creative_RawLogs>();

            var result = Store.Query<Creative_RawLogs.ReduceResult, Creative_RawLogs>(x => x.DroneId == "drone1");

            result.Should().Contain(x => x.DroneId == "drone1");
            result.Should().OnlyContain(x => x.RawLogs.Count() == 5);
            result[0].RawLogs.Select(x => x.Message).Should().BeEquivalentTo(new[] { "message 1", "message 2", "message 3", "message 4", "message 5" });
        }
    }
}
