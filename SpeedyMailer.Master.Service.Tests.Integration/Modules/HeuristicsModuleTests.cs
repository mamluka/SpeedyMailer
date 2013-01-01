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

			Store.Store(new DeliverabilityClassificationRules
							{
								Rules = new List<HeuristicRule>
									{
										new HeuristicRule { Condition = "yeah",Type = Classification.HardBounce},
										new HeuristicRule { Condition = "sexy",Type = Classification.TempBlock,Data = new HeuristicData { TimeSpan = TimeSpan.FromHours(2) }},
									}
							});

			Store.WaitForEntitiesToExist<DeliverabilityClassificationRules>();

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, DeliverabilityClassificationRules>();

			result.Rules.Should().Contain(x => x.Condition == "yeah" && x.Type == Classification.HardBounce);
			result.Rules.Should().Contain(x => x.Condition == "sexy" && x.Type == Classification.TempBlock);
		}

		[Test]
		public void SetDeliveryRules_WhenCalled_ShouldSaveTheHeuristicsRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.DeliverabilityClassificationRules = new DeliverabilityClassificationRules
			{
				Rules = new List<HeuristicRule>
									{
										new HeuristicRule { Condition = "yeah",Type = Classification.HardBounce},
										new HeuristicRule { Condition = "sexy",Type = Classification.TempBlock,Data = new HeuristicData { TimeSpan = TimeSpan.FromHours(2) }},
									}
			});

			Store.WaitForEntitiesToExist<DeliverabilityClassificationRules>();

			var result = Store.Query<DeliverabilityClassificationRules>().SingleOrDefault();

			result.Rules.Should().Contain(x => x.Condition == "yeah" && x.Type == Classification.HardBounce);
			result.Rules.Should().Contain(x => x.Condition == "sexy" && x.Type == Classification.TempBlock);
		}

		[Test]
		public void SetDeliveryRules_WhenRulesAlreadyExusts_ShouldSOverrideHeuristicsRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			Store.Store(new DeliverabilityClassificationRules
							{
								Rules = new List<HeuristicRule>
									{
										new HeuristicRule { Condition = "old",Type = Classification.HardBounce},
										new HeuristicRule { Condition = "very old",Type = Classification.TempBlock,Data = new { TimeSpan = TimeSpan.FromHours(2) }},
									}
							});

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.DeliverabilityClassificationRules = new DeliverabilityClassificationRules
			{
				Rules = new List<HeuristicRule>
									{
										new HeuristicRule { Condition = "yeah",Type = Classification.HardBounce},
										new HeuristicRule { Condition = "sexy",Type = Classification.TempBlock,Data = new { TimeSpan = TimeSpan.FromHours(2) }},
									}
			});

			Store.WaitForEntitiesToExist<DeliverabilityClassificationRules>();

			var result = Store.Query<DeliverabilityClassificationRules>().SingleOrDefault();

			result.Rules.Should().Contain(x => x.Condition == "yeah" && x.Type == Classification.HardBounce);
			result.Rules.Should().Contain(x => x.Condition == "sexy" && x.Type == Classification.TempBlock);
		}
	}
}
