using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Core.IntegrationTests.Utilities
{
	[TestFixture]
	public class ApiTests : IntegrationTestBase
	{
		private Api _target;

		public override void ExtraSetup()
		{
			_target = MasterResolve<Api>();
		}

		[Test]
		public void Call_WhenUsingPost_ShouldCallTheEndpoint()
		{
			ServiceActions.EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			ListenToApiCall<TestApi, TestApi.Request>();

			_target.Call<TestApi>(call =>
								  call.WithParameters(p =>
													  p.CallId = "testing call"
									))
				.Post();

			AssertApiCalled<TestApi.Request>(x => x.CallId == "testing call");
		}

		[Test]
		public void Call_WhenUsingGet_ShouldCallTheEndpoint()
		{
			ServiceActions.EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			ListenToApiCall<TestApi, TestApi.Request>();

			_target.Call<TestApi>(call =>
								  call.WithParameters(p =>
													  p.CallId = "testing call"
									))
				.Get();

			AssertApiCalled<TestApi.Request>(x => x.CallId == "testing call");
		}
	}

	public class TestApi : ApiCall<TestApi.Request>
	{
		public TestApi() : base("/testing/api") { }

		public class Request
		{
			public string CallId { get; set; }
		}
	}



}
