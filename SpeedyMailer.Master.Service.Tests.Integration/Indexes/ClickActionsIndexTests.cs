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
	public class ClickActionsIndexTests : IntegrationTestBase
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
									                               new ClickAction {ContactId = "contact/1", CreativeId = "creative/1"},
									                               new ClickAction {ContactId = "contact/2", CreativeId = "creative/1"}
								                               }
						                },
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/100", CreativeId = "creative/1"},
									                               new ClickAction {ContactId = "contact/200", CreativeId = "creative/1"}
								                               }
						                },
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/1000", CreativeId = "creative/2"},
									                               new ClickAction {ContactId = "contact/2000", CreativeId = "creative/2"}
								                               }
						                },
					                new DroneStateSnapshoot
						                {
							                ClickActions = new List<ClickAction>
								                               {
									                               new ClickAction {ContactId = "contact/4000", CreativeId = "creative/2"},
									                               new ClickAction {ContactId = "contact/5000", CreativeId = "creative/2"}
								                               }
						                }
				                }.ToList();

			snapshots.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Creatives_ClickActions.ReduceResult,Creatives_ClickActions>();

			var result = Store.Query<Creatives_ClickActions.ReduceResult, Creatives_ClickActions>(x => x.CreativeId == "creative/1");

			result.Should().Contain(x => x.CreativeId == "creative/1");
			result.Should().OnlyContain(x => x.ClickedBy.Count() == 4);
		}
	}
}
