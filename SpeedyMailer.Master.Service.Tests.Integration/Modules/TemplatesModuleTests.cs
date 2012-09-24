using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class TemplatesModuleTests : IntegrationTestBase
	{
		[Test]
		public void UnsubscribeAdd_WhenCalled_ShouldAddTheTemplateToTheDatabase()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.CreateUnsubscribeTemplate>(x =>
																	 {
																		 x.Body = "body";
																	 });

			WaitForEntitiesToExist<Template>(1);

			var result = Query<Template>().First();

			result.Body.Should().Be("body");
			result.Type.Should().Be(TemplateType.Unsubscribe);
		}

		[Test]
		public void TemplatesGet_WhenCalled_ReturnTheStoredTemplates()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			Store(CreateTemplate("body"));
			Store(CreateTemplate("second body"));

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.GetTemplates, List<Template>>(x =>
														{
															x.Type = TemplateType.Unsubscribe;
														});


			result.Should().Contain(x => x.Body == "body" || x.Body == "second body");
		}

		private static Template CreateTemplate(string body)
		{
			return new Template
					   {
						   Type = TemplateType.Unsubscribe,
						   Body = body
					   };
		}
	}
}
