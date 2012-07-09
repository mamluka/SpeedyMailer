using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Ninject;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchCreativeFragmentsTaskTasts:IntegrationTestBase
	{
		[Test]
		public void Execute_WhenStarted_ShouldFetchACreativeFragmentFromServer()
		{
			DroneActions.EditSettings<IApiCallsSettings>(x=> x.ApiBaseUri = DefaultBaseUrl);
			ListenToApiCall<ServiceEndpoints.FetchFragment>();

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertApiCalled();
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldStartSendingEmails()
		{
			ApiResponse<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>(x=> x.CreativeFragment = new CreativeFragment() {Id = "testingId"});
			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);
		}
	}
}
