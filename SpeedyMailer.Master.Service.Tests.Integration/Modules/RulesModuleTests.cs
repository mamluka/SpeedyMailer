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
							                                                               new IntervalRule
								                                                               {
									                                                               Conditons = new List<string>
										                                                                          {
											                                                                          "gmail.com",
											                                                                          "googlemail.com",
										                                                                          },
									                                                               Interval = 5,
																								   Group = "gmail"
								                                                               }
						                                                               };
																	 });

			Store.WaitForEntitiesToExist<IntervalRule>(1);

			var result = Store.Query<IntervalRule>().First();

			result.Conditons.Should().Contain(new[] { "gmail.com", "googlemail.com" });
			result.Interval.Should().Be(5);
			result.Group.Should().Be("gmail");
		}
	}
}
