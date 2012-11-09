using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class RulesModuleTests : IntegrationTestBase
	{
		[Test]
		public void AddRules_WhenGivenARule_ShouldSaveIt()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Rules.AddIntervalRules>(x =>
				                                                  {
					                                                  x.IntervalRules = new List<IntervalRule>
						                                                                    {
							                                                                    CreateRule(new List<string> {"gmail.com", "googlemail.com",}, "gmail", 5)
						                                                                    };
				                                                  });

			Store.WaitForEntitiesToExist<IntervalRule>(1);

			var result = Store.Query<IntervalRule>().First();

			result.Conditons.Should().Contain(new[] { "gmail.com", "googlemail.com" });
			result.Interval.Should().Be(5);
			result.Group.Should().Be("gmail");
		}

		[Test]
		public void GetRules_WhenCalled_ShouldReturnIntervalRules()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			Store.Store(CreateRule(new[] { "gmail.com", "googlemail.com" }, "gmail", 5));
			Store.WaitForEntitiesToExist<IntervalRule>(1);

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.Rules.GetIntervalRules,List<IntervalRule>>();

			result[0].Conditons.Should().Contain(new[] { "gmail.com", "googlemail.com" });
			result[0].Interval.Should().Be(5);
			result[0].Group.Should().Be("gmail");
		}

		private static IntervalRule CreateRule(IEnumerable<string> conditions, string @group, int interval)
		{
			return new IntervalRule
				       {
					       Conditons = conditions.ToList(),
					       Interval = interval,
					       Group = @group
				       };
		}
	}
}
