using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	[TestFixture]
	public class ResumePausedGroupsTaskTests : IntegrationTestBase
	{
		public ResumePausedGroupsTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenThereArePausedGroupsThatHaveExpired_ShouldRemoveTheGroupPolicy()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new GroupsAndIndividualDomainsSendingPolicies
								   {
									   GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>
						                                          {
							                                          {
								                                          "gmail", new ResumeSendingPolicy
									                                                   {
										                                                   ResumeAt = DateTime.UtcNow - TimeSpan.FromHours(1)
									                                                   }
							                                          }
						                                          }
								   });

			DroneActions.StartScheduledTask(new ResumePausedGroupsTask());

			DroneActions.WaitForChangeOnStoredObject<GroupsAndIndividualDomainsSendingPolicies>(x => !x.GroupSendingPolicies.ContainsKey("gmail"));
		}

		[Test]
		public void Execute_WhenThereArePausedGroupsThatHaveExpired_ShouldRaiseAnEventOfResuming()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<ResumingGroups>();

			DroneActions.Store(new GroupsAndIndividualDomainsSendingPolicies
								   {
									   GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>
						                                          {
							                                          {
								                                          "gmail", new ResumeSendingPolicy
									                                                   {
										                                                   ResumeAt = DateTime.UtcNow - TimeSpan.FromHours(1)
									                                                   }
							                                          }
						                                          }
								   });

			var task = new ResumePausedGroupsTask();
			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<ResumingGroups>(x => x.Groups.Should().Contain(s => s == "gmail"));
		}
	}
}
