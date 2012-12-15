using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class UpdateIpReputationTests : IntegrationTestBase
	{
		public UpdateIpReputationTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Inspect_WhenIpIsBlocking_ShouldUpdateReputationForThatGroup()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(48) } }
				});

			FireEvent<UpdateIpReputation, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							DomainGroup = "yahoo",
							Message = "this is a block"
						}
				});

			var result = DroneActions.FindSingle<IpReputation>();

			result.GroupReputation.Should().ContainKey("yahoo");
			result.GroupReputation["yahoo"].Should().OnlyContain(x => x > DateTime.UtcNow.AddSeconds(-20));
		}

		[Test]
		public void Inspect_WhenIpIsBlockingAndTheGroupAlreadyExists_ShouldAddANewTimeStep()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(48) } }
				});

			DroneActions.Store(new IpReputation
				{
					GroupReputation = new Dictionary<string, List<DateTime>>
						{
							{ "yahoo",new List<DateTime> { new DateTime(1999,1,1,0,0,0,DateTimeKind.Utc)} }
						}
				});

			FireEvent<UpdateIpReputation, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							DomainGroup = "yahoo",
							Message = "this is a block"
						}
				});

			var result = DroneActions.FindSingle<IpReputation>();

			result.GroupReputation.Should().ContainKey("yahoo");
			result.GroupReputation["yahoo"].Should().Contain(x => x > DateTime.UtcNow.AddSeconds(-20));
			result.GroupReputation["yahoo"].Should().Contain(x => x == new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}
	}
}
