using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
	public class Creative_ActionsIndexTests : IntegrationTestBase
	{
		[Test]
		public void Index_WhenGivenSnapShots_ShouldMapReduceTheClicks()
		{
			var snapshots = new[]
				                {
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/1", CreativeId = "creative/1",Domain = "www.cool.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)},
									                               new ClickAction {ContactId = "contact/2", CreativeId = "creative/1",Domain = "www.cool.com",Date = new DateTime(2006,1,1,0,0,0,DateTimeKind.Utc)}
								                               }
						                },
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/100", CreativeId = "creative/1",Domain = "www.cool.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)},
									                               new ClickAction {ContactId = "contact/200", CreativeId = "creative/1",Domain = "www.sexy.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)}
								                               }
						                },
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/1000", CreativeId = "creative/2",Domain = "www.cool.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)},
									                               new ClickAction {ContactId = "contact/2000", CreativeId = "creative/2",Domain = "www.cool.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)}
								                               }
						                },
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/4000", CreativeId = "creative/2",Domain = "www.cool.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)},
									                               new ClickAction {ContactId = "contact/5000", CreativeId = "creative/2",Domain = "www.cool.com",Date = new DateTime(2005,1,1,0,0,0,DateTimeKind.Utc)}
								                               }
						                }
				                }.ToList();

			snapshots.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Creative_ClickActions.ReduceResult, Creative_ClickActions>();

			var result = Store.Query<Creative_ClickActions.ReduceResult, Creative_ClickActions>(x => x.CreativeId == "creative/1");

			result.Should().Contain(x => x.CreativeId == "creative/1");
			result.Should().OnlyContain(x => x.ClickedBy.Count() == 4);
			result[0].ClickedBy.Select(x => x.Domain).Should().BeEquivalentTo(new[] { "www.cool.com", "www.sexy.com" });
			result[0].ClickedBy.Select(x => x.Time).Should().BeEquivalentTo(new[] { new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
		}
	}
}
