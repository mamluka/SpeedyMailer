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
		public void Call_WhenCalled_ShouldCallTheEndpoint()
		{
			ServiceActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			ListenToApiCall<PostTestApi>();

			_target.Call<PostTestApi>(x => x.CallId = "testing call");

			AssertApiCalled<PostTestApi>(x => x.CallId == "testing call");
		}
		
	}

	public class PostTestApi : ApiCall
	{
		public PostTestApi() : base("/testing/api")
		{
			CallMethod = RestMethod.Post;
		}

		public string CallId { get; set; }

		public class Response
		{
			public string WhatWeGet { get; set; }
		}

	}

}
