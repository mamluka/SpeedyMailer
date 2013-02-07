using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Logging;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	[TestFixture]
	public class BroadcastDroneToServiceTaskTests : IntegrationTestBase
	{

		public BroadcastDroneToServiceTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		private IScheduledTaskManager _scheduledTaskManager;

		public override void ExtraSetup()
		{
			_scheduledTaskManager = DroneResolve<IScheduledTaskManager>();
		}

		[Test]
		public void Start_WhenCalled_ShouldContactTheMasterServiceAndRegisterTheDrone()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														{
															x.Identifier = "drone1";
															x.BaseUrl = "http://192.168.1.1:2589";
															x.Domain = "example.com";
														});

			DroneActions.EditSettings<ApiCallsSettings>(x =>
															{
																x.ApiBaseUri = DefaultBaseUrl;
															});

			Api.ListenToApiCall<ServiceEndpoints.Drones.RegisterDrone>();

			var task = new BroadcastDroneToServiceTask();
			_scheduledTaskManager.AddAndStart(task);

			Api.AssertApiCalled<ServiceEndpoints.Drones.RegisterDrone>(x =>
															x.Identifier == "drone1" &&
															x.BaseUrl == "http://192.168.1.1:2589" &&
															x.Domain == "example.com"
				);
		}

		[Test]
		public void Start_WhenCalled_ShouldBroadCastAllExceptions()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														{
															x.Identifier = "drone1";
															x.BaseUrl = "http://192.168.1.1:2589";
															x.Domain = "example.com";
															x.StoreHostname = DefaultHostUrl;
														});

			DroneActions.EditSettings<ApiCallsSettings>(x =>
															{
																x.ApiBaseUri = DefaultBaseUrl;
															});

			Api.ListenToApiCall<ServiceEndpoints.Drones.RegisterDrone>();

			DroneActions.Store(new DroneExceptionLogEntry { component = "c", message = "message", time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), exception = "exception" });

			var task = new BroadcastDroneToServiceTask();
			_scheduledTaskManager.AddAndStart(task);

			Api.AssertApiCalled<ServiceEndpoints.Drones.RegisterDrone>(x => x.Exceptions[0].Component == "c" &&
																			x.Exceptions[0].Exception == "exception" &&
																			x.Exceptions[0].Message == "message" &&
																			x.Exceptions[0].Time == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString()
				);
		}

		[Test]
		public void Start_WhenCalled_ShouldPostTheReputationToTheMaster()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														{
															x.Identifier = "drone1";
															x.BaseUrl = "http://192.168.1.1:2589";
															x.Domain = "example.com";
															x.StoreHostname = DefaultHostUrl;
														});

			DroneActions.EditSettings<ApiCallsSettings>(x =>
															{
																x.ApiBaseUri = DefaultBaseUrl;
															});

			Api.ListenToApiCall<ServiceEndpoints.Drones.RegisterDrone>();

			DroneActions.Store(new IpReputation
				{
					BlockingHistory = new Dictionary<string, List<DateTime>> { { "gmail", new List<DateTime> { new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) } } },
					ResumingHistory = new Dictionary<string, List<DateTime>> { { "gmail", new List<DateTime> { new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc) } } }
				});

			var task = new BroadcastDroneToServiceTask();
			_scheduledTaskManager.AddAndStart(task);

			Api.AssertApiCalled<ServiceEndpoints.Drones.RegisterDrone>(x =>
																	   x.IpReputation.BlockingHistory["gmail"][0] == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
																	   x.IpReputation.ResumingHistory["gmail"][0] == new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc)
				);
		}

		[Test]
		public void Start_WhenExecuted_ShouldSendTotalUnprocessedPackages()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.Identifier = "drone1";
				x.BaseUrl = "http://192.168.1.1:2589";
				x.Domain = "example.com";
				x.StoreHostname = DefaultHostUrl;
			});

			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			DroneActions.StoreCollection(new[]
				{
					new CreativePackage { Processed = false },
					new CreativePackage { Processed = false },
					new CreativePackage { Processed = true }
				});


			Api.ListenToApiCall<ServiceEndpoints.Drones.RegisterDrone>();

			DroneActions.StartScheduledTask(new BroadcastDroneToServiceTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.RegisterDrone>(x => x.Identifier == "drone1" &&
																				x.BaseUrl == "http://192.168.1.1:2589" &&
																				x.SendingStatus.UnprocessedPackages == 2);
		}

		[Test]
		public void Start_WhenExecuted_ShouldSendGroupInformation()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.Identifier = "drone1";
				x.BaseUrl = "http://192.168.1.1:2589";
				x.Domain = "example.com";
				x.StoreHostname = DefaultHostUrl;
			});

			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			DroneActions.StoreCollection(new[]
				{
					new CreativePackage { Processed = false,Group = "gmail"},
					new CreativePackage { Processed = false,Group = "gmail"},
					new CreativePackage { Processed = false,Group = "gmail"},
					new CreativePackage { Processed = false,Group = "hotmail"},
					new CreativePackage { Processed = false,Group = "hotmail"}
				});


			Api.ListenToApiCall<ServiceEndpoints.Drones.RegisterDrone>();

			DroneActions.StartScheduledTask(new BroadcastDroneToServiceTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.RegisterDrone>(x => x.Identifier == "drone1" &&
																				x.BaseUrl == "http://192.168.1.1:2589" &&
																				x.SendingStatus.Groups.Any(p => p.Name == "gmail" && p.Total == 3) &&
																				x.SendingStatus.Groups.Any(p => p.Name == "hotmail" && p.Total == 2));
		}
	}
}