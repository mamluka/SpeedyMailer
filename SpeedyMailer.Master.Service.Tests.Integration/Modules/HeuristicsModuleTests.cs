using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class HeuristicsModuleTests : IntegrationTestBase
	{
		[Test]
		public void GetDeliveryRules_WhenCalled_ShouldReturnTheHeuristicsRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			Store.Store(new UnDeliveredMailClassificationHeuristicsRules
							{
								HardBounceRules = new List<HeuristicRule> { new HeuristicRule { Condition = "yeah", TimeSpan = TimeSpan.FromHours(2) } },
								IpBlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "sexy", TimeSpan = TimeSpan.FromHours(2) } }
							});

			Store.WaitForEntitiesToExist<UnDeliveredMailClassificationHeuristicsRules>(1);

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, UnDeliveredMailClassificationHeuristicsRules>();

			result.HardBounceRules.Should().Contain(x=> x.Condition == "yeah" && x.TimeSpan == TimeSpan.FromHours(2));
			result.IpBlockingRules.Should().Contain(x=> x.Condition == "sexy" && x.TimeSpan == TimeSpan.FromHours(2));
		}

		[Test]
		public void SetDeliveryRules_WhenCalled_ShouldSaveTheHeuristicsRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.Rules = new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule> { new HeuristicRule { Condition = "yeah", TimeSpan = TimeSpan.FromHours(2) } },
				IpBlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "sexy", TimeSpan = TimeSpan.FromHours(2) } }
			});

			Store.WaitForEntitiesToExist<UnDeliveredMailClassificationHeuristicsRules>();

			var result = Store.Query<UnDeliveredMailClassificationHeuristicsRules>().SingleOrDefault();

			result.HardBounceRules.Should().Contain(x => x.Condition == "yeah" && x.TimeSpan == TimeSpan.FromHours(2));
			result.IpBlockingRules.Should().Contain(x => x.Condition == "sexy" && x.TimeSpan == TimeSpan.FromHours(2));
		}
		
		[Test]
		public void SetDeliveryRules_WhenRulesAlreadyExusts_ShouldSOverrideHeuristicsRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			Store.Store(new UnDeliveredMailClassificationHeuristicsRules
				            {
								HardBounceRules = new List<HeuristicRule> { new HeuristicRule { Condition = "old", TimeSpan = TimeSpan.FromHours(2) } },
								IpBlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "very old", TimeSpan = TimeSpan.FromHours(2) } }
				            });

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.Rules = new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule> { new HeuristicRule { Condition = "yeah", TimeSpan = TimeSpan.FromHours(2) } },
				IpBlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "sexy", TimeSpan = TimeSpan.FromHours(2) } }
			});

			Store.WaitForEntitiesToExist<UnDeliveredMailClassificationHeuristicsRules>();

			var result = Store.Query<UnDeliveredMailClassificationHeuristicsRules>().SingleOrDefault();

			result.HardBounceRules.Should().Contain(x => x.Condition == "yeah" && x.TimeSpan == TimeSpan.FromHours(2));
			result.IpBlockingRules.Should().Contain(x => x.Condition == "sexy" && x.TimeSpan == TimeSpan.FromHours(2));
		}
	}
}
