using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchDeliveryClassificationHeuristicsTaskTests : IntegrationTestBase
	{
		public FetchDeliveryClassificationHeuristicsTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenStarted_ShouldFetchTheHeuristics()
		{
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Heuristics.Delivery,
				UnDeliveredMailClassificationHeuristicsRules>(x =>
					                                              {
						                                              x.HardBounceRules = new List<string>
							                                                                  {
								                                                                  "hard bounce rule"
							                                                                  };
						                                              x.IpBlockingRules = new List<string>
							                                                                  {
								                                                                  "blocking rule"
							                                                                  };
					                                              });

			var task = new FetchDeliveryClassificationHeuristicsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<UnDeliveredMailClassificationHeuristicsRules>();

			var result = DroneActions.FindSingle<UnDeliveredMailClassificationHeuristicsRules>();

			result.HardBounceRules.Should().Contain("hard bounce rule");
			result.IpBlockingRules.Should().Contain("blocking rule");
		}
	}
}
