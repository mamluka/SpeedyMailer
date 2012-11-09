using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchIntervalRulesTaskTests : IntegrationTestBase
	{
		public FetchIntervalRulesTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenIntervalRulesArePresent_ShouldFetchThem()
		{
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Rules.GetIntervalRules, List<IntervalRule>>(x =>
																									{
																										x.Add(CreateRule("gmail", "gmail.com"));
																										x.Add(CreateRule("hotmail", "hotmail.com"));
																										x.Add(CreateRule("aol", "aol.com"));
																									}
				);

			var task = new FetchIntervalRulesTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<IntervalRule>();

			var result = DroneActions.FindAll<IntervalRule>();

			result.Should().HaveCount(3);

			var domainGroups = result.Select(x => x.Group).ToList();
			domainGroups.Should().BeEquivalentTo(new[] { "gmail", "hotmail", "aol" });

		}

		private static IntervalRule CreateRule(string groupDomain, string condition)
		{
			return new IntervalRule
					   {
						   Group = groupDomain,
						   Conditons = new List<string>()
						                   {
							                   condition
						                   },
						   Interval = 10
					   };
		}
	}
}
