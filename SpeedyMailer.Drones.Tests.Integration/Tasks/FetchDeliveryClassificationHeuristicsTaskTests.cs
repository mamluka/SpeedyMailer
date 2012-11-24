using System;
using System.Collections.Generic;
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
																	  x.HardBounceRules = new List<HeuristicRule>
							                                                                  {
								                                                                  new HeuristicRule
									                                                                  {
										                                                                 Condition  = "hard bounce rule",
																										 TimeSpan = TimeSpan.FromHours(4)
									                                                                  } 
							                                                                  };
																	  x.IpBlockingRules = new List<HeuristicRule>
							                                                                  {
																								  new HeuristicRule
									                                                                  {
										                                                                 Condition  = "blocking rule",
																										 TimeSpan = TimeSpan.FromHours(4)
									                                                                  } 
							                                                                  };
																  });

			var task = new FetchDeliveryClassificationHeuristicsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<UnDeliveredMailClassificationHeuristicsRules>();

			var result = DroneActions.FindSingle<UnDeliveredMailClassificationHeuristicsRules>();

			result.HardBounceRules.Should().Contain(x => x.Condition == "hard bounce rule" && x.TimeSpan == TimeSpan.FromHours(4));
			result.IpBlockingRules.Should().Contain(x => x.Condition == "blocking rule" && x.TimeSpan == TimeSpan.FromHours(4));
		}

		[Test]
		public void Execute_WhenCalledTwice_ShouldUpdateTheRules()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Heuristics.GetDeliveryRules,
				UnDeliveredMailClassificationHeuristicsRules>(x =>
																  {
																	  x.HardBounceRules = new List<HeuristicRule>
							                                                                  {
																								  new HeuristicRule
									                                                                  {
										                                                                 Condition  = "hard bounce rule",
																										 TimeSpan = TimeSpan.FromHours(4)
									                                                                  } 
								                                                                  
							                                                                  };
																	  x.IpBlockingRules = new List<HeuristicRule>
							                                                                  {
																								  new HeuristicRule
									                                                                  {
										                                                                 Condition  = "blocking rule",
																										 TimeSpan = TimeSpan.FromHours(4)
									                                                                  } 
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
