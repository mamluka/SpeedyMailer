using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

			DroneActions.Store(new GroupsSendingPolicies
								   {
									   GroupSendingPolicies = new Dictionary<string, GroupSendingPolicy>
						                                          {
							                                          {
								                                          "gmail", new GroupSendingPolicy
									                                                   {
										                                                   ResumeAt = DateTime.UtcNow - TimeSpan.FromHours(1)
									                                                   }
							                                          }
						                                          }
								   });

			DroneActions.StartScheduledTask(new ResumePausedGroupsTask());

			DroneActions.WaitForChangeOnStoredObject<GroupsSendingPolicies>(x => !x.GroupSendingPolicies.ContainsKey("gmail"));
		}
	}
}
