using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;
using nDumbster;
using nDumbster.smtp;
using System.Linq;

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
			DroneActions.EditSettings<IEmailingSettings>(x=> x.WriteEmailsToDisk=true);

			PrepareApiResponse<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>(x =>
					x.CreativeFragment = new CreativeFragment
											{
												Id = "fragment/1",
												Creative = new Creative
															{
																Body = "hello world email",
																Subject = "hello world subject",
															},
												Recipients = new List<Contact>
												             	{
												             		new Contact
												             			{
												             				Email = "test@test.com"
												             			}
												             	}
											}
				);

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertEmailSent();
		}

		private void AssertEmailSent()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			while (!Directory.GetFiles(AssemblyDirectory).Any(x => x.StartsWith("email")) && stopWatch.ElapsedMilliseconds < 30 * 1000)
			{
				
			};

			Assert.True(Directory.GetFiles(AssemblyDirectory).Any(x => x.StartsWith("email")));
		}
	}
}
