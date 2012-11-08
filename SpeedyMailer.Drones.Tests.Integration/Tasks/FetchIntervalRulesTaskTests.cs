using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchIntervalRulesTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenIntervalRulesArePresent_ShouldFetchThem()
		{
			Api.PrepareApiResponse<ServiceEndpoints.Rules.GetIntervalRules, List<IntervalRule>>(x =>
				                                                                                    {
					                                                                                    x.Add(CreateRule("gmail", "gmail.com"));
					                                                                                    x.Add(CreateRule("gmail", "gmail.com"));
					                                                                                    x.Add(CreateRule("gmail", "gmail.com"));
				                                                                                    }
				);
		}

		private static IntervalRule CreateRule(string groupDomain, string condition)
		{
			return new IntervalRule
				       {
					       Group = groupDomain,
					       Conditons = new[]
						                   {
							                   condition
						                   },
					       Interval = 10
				       };
		}
	}
}
