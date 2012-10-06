using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
									                                                               Conditon = new List<string>
										                                                                          {
											                                                                          "gmail.com",
											                                                                          "hotmail.com"
										                                                                          },
									                                                               Interval = 5
								                                                               }
						                                                               };
																	 });

			WaitForEntitiesToExist<IntervalRule>(1);

			var result = Query<IntervalRule>().First();

			result.Conditon.Should().Contain(new[] { "gmail.com", "hotmail.com" });
			result.Interval.Should().Be(5);
		}
	}
}
