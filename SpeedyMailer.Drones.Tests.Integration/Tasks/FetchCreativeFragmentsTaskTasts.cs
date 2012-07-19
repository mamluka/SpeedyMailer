using System.Collections.Generic;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;
using nDumbster;
using nDumbster.smtp;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchCreativeFragmentsTaskTasts : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenStarted_ShouldFetchACreativeFragmentFromServer()
		{
			DroneActions.EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			ListenToApiCall<ServiceEndpoints.FetchFragment>();

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertApiCalled<ServiceEndpoints.FetchFragment>();
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldStartSendingEmails()
		{
			PrepareApiResponse<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>(x =>
					x.CreativeFragment = new CreativeFragment
											{
												Id = "fragment/1",
												Creative = new Creative
															{
																Body = "hell world email",
																Subject = "hello world subject",
															},
												Recipients = new List<Contact>
												             	{
												             		new Contact {}
												             	}
											}
				);

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);
		}
	}
}
