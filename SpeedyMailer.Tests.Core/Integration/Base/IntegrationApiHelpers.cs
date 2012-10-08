using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Nancy.Hosting.Self;
using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationApiHelpers
	{
		private readonly string _defaultBaseUrl;
		private NancyHost _nancy;

		public IntegrationApiHelpers(string defaultBaseUrl)
		{
			_defaultBaseUrl = defaultBaseUrl;
		}

		public void ListenToApiCall<TEndpoint>(string endpointBaseUrl = "") where TEndpoint : ApiCall, new()
		{
			StartGeneticEndpoint<TEndpoint, NoResponse>(endpointBaseUrl, new NoResponse());
		}

		private void StartGeneticEndpoint<TEndpoint, TResponse>(string endpointBaseUrl, TResponse response)
			where TEndpoint : ApiCall, new()
			where TResponse : class
		{
			var endpoint = new TEndpoint().Endpoint;
			var uri = _defaultBaseUrl ?? endpointBaseUrl;
			RestCallTestingModule<TEndpoint, TResponse>.Model = default(TEndpoint);
			RestCallTestingModule<TEndpoint, TResponse>.WasCalled = false;
			RestCallTestingModule<TEndpoint, TResponse>.Files = null;

			var restCallTestingBootstrapper = new RestCallTestingBootstrapper<TEndpoint, TResponse>(endpoint, response);
			_nancy = new NancyHost(new Uri(uri), restCallTestingBootstrapper);
			_nancy.Start();

			Trace.WriteLine("Nancy started on: " + uri);
		}

		public void PrepareApiResponse<TEndpoint, TResponse>(Action<TResponse> action, string endpointBaseUrl = "")
			where TResponse : class, new()
			where TEndpoint : ApiCall, new()
		{
			var response = new TResponse();
			action(response);
			StartGeneticEndpoint<TEndpoint, TResponse>(endpointBaseUrl, response);
		}

		public void PrepareApiResponse<TEndpoint, TResponse>(TResponse response, string endpointBaseUrl = "")
			where TResponse : class, new()
			where TEndpoint : ApiCall, new()
		{
			StartGeneticEndpoint<TEndpoint, TResponse>(endpointBaseUrl, response);
		}

		public void AssertApiCalled<TEndpoint>(Func<TEndpoint, bool> func, int seconds = 30) where TEndpoint : class
		{
			WaitForApiToBeCalled<TEndpoint>(seconds);

			if (RestCallTestingModule<TEndpoint, NoResponse>.Model != null)
			{
				Assert.That(func(RestCallTestingModule<TEndpoint, NoResponse>.Model), Is.True);
				return;
			}
			Assert.Fail("REST call was not executed in the ellapsed time");
		}

		public void AssertFilesUploaded<TEndpoint>(IEnumerable<string> files, int seconds = 30) where TEndpoint : class
		{
			WaitForApiToBeCalled<TEndpoint>(seconds);

			if (RestCallTestingModule<TEndpoint, NoResponse>.Files != null)
			{
				Assert.That(RestCallTestingModule<TEndpoint, NoResponse>.Files, Is.EquivalentTo(files.ToArray()));
				return;
			}

			Assert.Fail("REST call did not recieve the files");
		}

		public void AssertApiCalled<TEndpoint>(int seconds = 30) where TEndpoint : class
		{
			WaitForApiToBeCalled<TEndpoint>(seconds);

			Assert.That(RestCallTestingModule<TEndpoint, NoResponse>.WasCalled, Is.True);
		}

		public void AssertApiWasntCalled<TEndpoint>(int seconds = 30) where TEndpoint : class
		{
			WaitForApiToBeCalled<TEndpoint>(seconds);

			Assert.That(RestCallTestingModule<TEndpoint, NoResponse>.WasCalled, Is.False);
		}

		public void AssertApiWasntCalled(int seconds = 30)
		{
			WaitForApiToBeCalled<NoResponse>(seconds);

			Assert.That(RestCallTestingModule<NoRequest, NoResponse>.WasCalled, Is.False);
		}

		private void WaitForApiToBeCalled<TEndpoint>(int seconds) where TEndpoint : class
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			while (RestCallTestingModule<TEndpoint, NoResponse>.WasCalled == false && stopwatch.ElapsedMilliseconds < seconds * 1000)
			{
				Thread.Sleep(500);
			}
		}

		public void StopListeningToApiCalls()
		{
			if (_nancy == null)
				return;

			_nancy.Stop();

			Trace.WriteLine("Nancy stopped on: " + _defaultBaseUrl);
		}
	}
}
