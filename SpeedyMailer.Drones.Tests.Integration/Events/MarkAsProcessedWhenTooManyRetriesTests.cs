using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class MarkAsProcessedWhenTooManyRetriesTests:IntegrationTestBase
	{
		public MarkAsProcessedWhenTooManyRetriesTests():base(x=> x.UseMongo = true)
		{ }


		[Test]
		public void Inspect_WhenGroupWasResumedThreeTimesInTheLastWeek_ShouldTurnThatDomainToProcessed()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.StoreCollection(new []
				{
					CreatePackage(),
					CreatePackage()
				});

			DroneActions.Store(new IpReputation
				{
					BlockingHistory = new Dictionary<string, List<DateTime>>
						{
							{"gmail", new List<DateTime> {DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)}}
						},
					ResumingHistory = new Dictionary<string, List<DateTime>>
						{
							{"gmail", new List<DateTime> {DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddHours(-3)}}
						}
				});
			
			FireEvent<MarkAsProcessedWhenTooManyRetries, ResumingGroups>(x =>
			                                                x.Groups = new List<string>
				                                                {
																	"gmail"
				                                                });

			var result = DroneActions.FindAll<CreativePackage>();

			result.Should().OnlyContain(x => x.Processed);
		}
		
		[Test]
		public void Inspect_WhenGroupWasResumedLessThemThreeTimes_ShouldDoNothing()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.StoreCollection(new []
				{
					CreatePackage(),
					CreatePackage()
				});

			DroneActions.Store(new IpReputation
				{
					BlockingHistory = new Dictionary<string, List<DateTime>>
						{
							{"gmail", new List<DateTime> {DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2)}}
						},
					ResumingHistory = new Dictionary<string, List<DateTime>>
						{
							{"gmail", new List<DateTime> {DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)}}
						}
				});
			
			FireEvent<MarkAsProcessedWhenTooManyRetries, ResumingGroups>(x =>
			                                                x.Groups = new List<string>
				                                                {
																	"gmail"
				                                                });

			var result = DroneActions.FindAll<CreativePackage>();

			result.Should().OnlyContain(x => x.Processed == false);
		}

		private CreativePackage CreatePackage()
		{
			return new CreativePackage
				{
					To = "david@gmail.com",
					Group = "gmail"
				};
		}
	}
}
