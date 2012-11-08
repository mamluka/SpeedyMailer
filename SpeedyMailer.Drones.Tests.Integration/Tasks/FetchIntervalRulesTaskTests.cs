using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchIntervalRulesTaskTests:IntegrationTestBase
	{
		[Test]
		public void Execute_WhenIntervalRulesArePresent_ShouldFetchThem()
		{
			Api.PrepareApiResponse<ServiceEndpoints.Rules.GetIntervalRules,IList<InterR>>();
		}
	}
}
