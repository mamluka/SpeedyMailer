using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
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
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Heuristics.GetDeliveryRules,
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
		
		[Test]
		public void Execute_WhenCalledTwice_ShouldUpdateTheRules()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Heuristics.GetDeliveryRules,
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

			DroneActions.StartScheduledTask(task);
			DroneActions.WaitForDocumentToExist<UnDeliveredMailClassificationHeuristicsRules>();

			var result = DroneActions.FindAll<UnDeliveredMailClassificationHeuristicsRules>();

			result.Should().HaveCount(1);
		}
	}
}
