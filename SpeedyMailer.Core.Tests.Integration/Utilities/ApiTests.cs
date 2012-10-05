using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Core.Apis;
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
			ServiceActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			_target = MasterResolve<Api>();
		}

		[Test]
		public void Call_WhenCalled_ShouldCallTheEndpoint()
		{
			ListenToApiCall<PostTestApi>();

			_target.Call<PostTestApi>(x => x.CallId = "testing call");

			AssertApiCalled<PostTestApi>(x => x.CallId == "testing call");
		}

		[Test]
		public void Call_WhenThereAreFiles_ShouldSendThem()
		{
			ListenToApiCall<PutFileApi>();
			CreateFile("for-api-send.text");


			_target
				.AddFiles(new[] { "for-api-send.text" })
				.Call<PutFileApi>(x => x.CallId = "testing file upload");

            AssertApiCalled<PutFileApi>(x => x.CallId == "testing file upload");
			AssertFilesUploaded<PutFileApi>(new[] {"for-api-send.text"});
		}
        
		private void CreateFile(string fileName)
		{
			using (var stream = new StreamWriter(fileName))
			{
				stream.WriteLine("this is a file uploading test");
				stream.Flush();
			}
		}
	}

	public class PutFileApi : ApiCall
	{
		public string CallId { get; set; }

		public PutFileApi()
			: base("/testing/put-file")
		{
			CallMethod = RestMethod.Put;
		}
	}

	public class PostTestApi : ApiCall
	{
		public PostTestApi()
			: base("/testing/api")
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
