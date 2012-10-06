using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class RulesModuleTests:IntegrationTestBase
	{
		[Test]
		public void AddRules_WhenGivenARule_ShouldSaveIt()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Rules.Save>(x=>
				                                      {
					                                      x.Action = RuleAction.Categorize;
					                                      x.What = new What
						                                               {
																		   Type = WhatType.Match,
																		   Conditions = new List<string>
																			                {
																				                "gmail.com",
																								"hotmail.com"
																			                }
						                                               };

				                                      });


		}
	}
}
