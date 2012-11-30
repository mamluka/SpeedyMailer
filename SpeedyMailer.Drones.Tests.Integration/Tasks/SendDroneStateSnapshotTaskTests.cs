using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	[TestFixture]
	public class SendDroneStateSnapshotTaskTests : IntegrationTestBase
	{
		public SendDroneStateSnapshotTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenExecuted_ShouldSendTheBasicDroneDetails()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														 {
															 x.Identifier = "drone1";
															 x.BaseUrl = "http://base.com";
															 x.StoreHostname = DefaultHostUrl;
														 });

			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);


			Api.ListenToApiCall<ServiceEndpoints.Drones.SendStateSnapshot>();

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.Drone.Id == "drone1" &&
																				x.Drone.BaseUrl == "http://base.com");
		}

		[Test]
		public void Execute_WhenExecuted_ShouldSendAllOfTheLogFilesCurrentlyInStore()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														 {
															 x.Identifier = "drone1";
															 x.BaseUrl = "http://base.com";
															 x.StoreHostname = DefaultHostUrl;
														 });

			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);


			Api.ListenToApiCall<ServiceEndpoints.Drones.SendStateSnapshot>();

			DroneActions.StoreCollection(new[]
				                             {
					                             new MailLogEntry { msg = "message 1" },
					                             new MailLogEntry { msg = "message 2" },
				                             });

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.RawLogs[0].Message == "message 1" &&
																				x.RawLogs[1].Message == "message 2");
		}

		[Test]
		public void Execute_WhenExecuted_ShouldSendAllOfTheProcessedLogsThatWereDecidedByTheDrone()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														 {
															 x.Identifier = "drone1";
															 x.BaseUrl = "http://base.com";
															 x.StoreHostname = DefaultHostUrl;
														 });

			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);


			Api.ListenToApiCall<ServiceEndpoints.Drones.SendStateSnapshot>();

			DroneActions.StoreCollection(new[]
				                             {
												 new MailSent { Recipient = "sent@sent.com" },
												 new MailSent { Recipient = "sent2@sent.com" }
				                             });

			DroneActions.StoreCollection(new[]
				                             {
												 new MailBounced { Recipient = "bounced@bounced.com" },
												 new MailBounced { Recipient = "bounce2d@bounced.com" }
				                             });

			DroneActions.StoreCollection(new[]
				                             {
												 new MailDeferred { Recipient = "deferred@deferred.com" },
												 new MailDeferred { Recipient = "deffered2@deferred.com" }
				                             });

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.MailSent[0].Recipient == "sent@sent.com" && x.MailSent[0].Recipient == "sent2@sent.com");
			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.MailBounced[0].Recipient == "bounced@bounced.com" && x.MailBounced[0].Recipient == "bounced2@bounced.com");
			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.MailDeferred[0].Recipient == "sent@sent.com" && x.MailSent[0].Recipient == "deferred2@deferred.com");
		}
	}
}
