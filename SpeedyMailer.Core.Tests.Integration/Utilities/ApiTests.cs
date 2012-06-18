using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Core.Api;
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
		private Api.Api _target;

		public override void ExtraSetup()
		{
			_target = MasterResolve<Api.Api>();
		}

		[Test]
		public void Call_WhenUsingPost_ShouldCallTheEndpoint()
		{
			ServiceActions.EditSettings<IApiCallsSettings>(x=> x.ApiBaseUri = ApiListningHostname);
			ListenToApiCall<TestApi,TestApi.Request>();

			_target.Call<TestApi>()
				.WithParameters(x => x.CallId = "testing call")
				.Post();

			AssertApiCall<TestApi.Request>(x=> x.CallId == "testing call");
		}

		[Test]
		public void Call_WhenUsingGet_ShouldCallTheEndpoint()
		{
			ServiceActions.EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = ApiListningHostname);
			ListenToApiCall<TestApi, TestApi.Request>();

			_target.Call<TestApi>()
				.WithParameters(x => x.CallId = "testing call")
				.Get();

			AssertApiCall<TestApi.Request>(x => x.CallId == "testing call");
		}
	}

	public class TestApi : ApiCall<TestApi.Request>
	{
		public class Request
		{
			public string CallId { get; set; }
		}

		public override string Endpoint
		{
			get { return "/testing/api"; }
		}
	}


	
}
