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
	public class Creative_RawLogsIndexTests : IntegrationTestBase
	{
//		[Test]
//		public void Index_WhenGivenSnapShots_ShouldMapReduceAllRawLogs()
//		{
//			var snapshots = new[]
//                {
//                    new DroneStateSnapshoot
//                        {
//                            RawLogs = new List<string> { "log1"}
//                        },
//						new DroneStateSnapshoot
//                        {
//                            RawLogs = new List<string> { "log2","log3"}
//                        },
//                }.ToList();
//
//			snapshots.ForEach(Store.Store);
//
//			Store.WaitForIndexNotToBeStale<Creative_RawLogs.ReduceResult, Creative_RawLogs>();
//
//			var result = Store.Query<Creative_RawLogs.ReduceResult, Creative_RawLogs>(x => x.Group == "All");
//
//			result[0].Logs.Should().BeEquivalentTo(new[] { "log1", "log2", "log3" });
//		}
	}
}
