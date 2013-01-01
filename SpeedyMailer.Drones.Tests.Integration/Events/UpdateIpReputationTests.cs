using System;
using System.Collections.Generic;
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
					Rules = new List<HeuristicRule>
						{
							new HeuristicRule { Condition = "this is a block",Data = new { TimeSpan = TimeSpan.FromHours(48)}}
						}
				});

			FireEvent<UpdateIpReputation, BlockingGroups>(x => x.Groups = new List<string>
				{
					"yahoo"
				});

			var result = DroneActions.FindSingle<IpReputation>();

			result.BlockingHistory.Should().ContainKey("yahoo");
			result.BlockingHistory["yahoo"].Should().OnlyContain(x => x > DateTime.UtcNow.AddSeconds(-20));
		}

		[Test]
		public void Inspect_WhenSendingIsResumed_ShouldUpdateReputationForThatGroup()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					Rules = new List<HeuristicRule>
						{
							new HeuristicRule { Condition = "this is a block",Data = new { TimeSpan = TimeSpan.FromHours(48)}}
						}
				});

			FireEvent<UpdateIpReputation, ResumingGroups>(x => x.Groups = new List<string>
				{
					"yahoo"
				});

			var result = DroneActions.FindSingle<IpReputation>();

			result.ResumingHistory.Should().ContainKey("yahoo");
			result.ResumingHistory["yahoo"].Should().OnlyContain(x => x > DateTime.UtcNow.AddSeconds(-20));
		}

		[Test]
		public void Inspect_WhenIpIsBlockingAndTheGroupAlreadyExists_ShouldAddANewTimeStep()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					Rules = new List<HeuristicRule>
						{
							new HeuristicRule { Condition = "this is a block",Data = new { TimeSpan = TimeSpan.FromHours(48)}}
						}
				});

			DroneActions.Store(new IpReputation
				{
					BlockingHistory = new Dictionary<string, List<DateTime>>
						{
							{ "yahoo",new List<DateTime> { new DateTime(1999,1,1,0,0,0,DateTimeKind.Utc)} }
						}
				});

			FireEvent<UpdateIpReputation, BlockingGroups>(x => x.Groups = new List<string>
				{
					"yahoo"
				});

			var result = DroneActions.FindSingle<IpReputation>();

			result.BlockingHistory.Should().ContainKey("yahoo");
			result.BlockingHistory["yahoo"].Should().Contain(x => x > DateTime.UtcNow.AddSeconds(-20));
			result.BlockingHistory["yahoo"].Should().Contain(x => x == new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void Inspect_WhenResumingGroupAlreadyExists_ShouldAddANewTimeStep()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					Rules = new List<HeuristicRule>
						{
							new HeuristicRule { Condition = "this is a block",Data = new { TimeSpan = TimeSpan.FromHours(48)}}
						}
				});

			DroneActions.Store(new IpReputation
				{
					ResumingHistory = new Dictionary<string, List<DateTime>>
						{
							{ "yahoo",new List<DateTime> { new DateTime(1999,1,1,0,0,0,DateTimeKind.Utc)} }
						}
				});

			FireEvent<UpdateIpReputation, ResumingGroups>(x => x.Groups = new List<string>
				{
					"yahoo"
				});

			var result = DroneActions.FindSingle<IpReputation>();

			result.ResumingHistory.Should().ContainKey("yahoo");
			result.ResumingHistory["yahoo"].Should().Contain(x => x > DateTime.UtcNow.AddSeconds(-20));
			result.ResumingHistory["yahoo"].Should().Contain(x => x == new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}
	}
}
