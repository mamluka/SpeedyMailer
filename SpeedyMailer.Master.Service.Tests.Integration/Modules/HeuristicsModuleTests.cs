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
								HardBounceRules = new List<string> { "yeah" },
								IpBlockingRules = new List<string> { "sexy" }
							});

			Store.WaitForEntitiesToExist<UnDeliveredMailClassificationHeuristicsRules>(1);

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, UnDeliveredMailClassificationHeuristicsRules>();

			result.HardBounceRules.Should().Contain(new[] { "yeah" });
			result.IpBlockingRules.Should().Contain(new[] { "sexy" });
		}

		[Test]
		public void SetDeliveryRules_WhenCalled_ShouldReturnTheHeuristicsRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.Rules = new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<string> { "yeah" },
				IpBlockingRules = new List<string> { "sexy" }
			});

			Store.WaitForEntitiesToExist<UnDeliveredMailClassificationHeuristicsRules>();

			var result = Store.Query<UnDeliveredMailClassificationHeuristicsRules>().SingleOrDefault();

			result.HardBounceRules.Should().Contain(new[] { "yeah" });
			result.IpBlockingRules.Should().Contain(new[] { "sexy" });
		}
	}
}
