using System;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Emails;
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

			DroneActions.StoreCollection(new[]
				                             {
					                             new MailLogEntry { msg = "message 1" },
					                             new MailLogEntry { msg = "message 2" },
				                             }, "log");


			Api.ListenToApiCall<ServiceEndpoints.Drones.SendStateSnapshot>();

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.Drone.Id == "drone1" &&
																				x.Drone.BaseUrl == "http://base.com");
		}

		[Test]
		public void Execute_WhenExecutedAndNoLogEventsWereFound_ShouldNotSendAnythingToTheMaster()
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

			Api.AssertApiWasntCalled<ServiceEndpoints.Drones.SendStateSnapshot>();
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
				                             }, "log");

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
					                             new MailLogEntry { msg = "message 1" },
					                             new MailLogEntry { msg = "message 2" },
				                             }, "log");

			DroneActions.StoreCollection(new[]
				                             {
												 new MailSent { Recipient = "sent@sent.com" },
												 new MailSent { Recipient = "sent2@sent.com" }
				                             });

			DroneActions.StoreCollection(new[]
				                             {
												 new MailBounced { Recipient = "bounced@bounced.com" },
												 new MailBounced { Recipient = "bounced2@bounced.com" }
				                             });

			DroneActions.StoreCollection(new[]
				                             {
												 new MailDeferred { Recipient = "deferred@deferred.com" },
												 new MailDeferred { Recipient = "deferred2@deferred.com" }
				                             });

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.MailSent[0].Recipient == "sent@sent.com" && x.MailSent[1].Recipient == "sent2@sent.com");
			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.MailBounced[0].Recipient == "bounced@bounced.com" && x.MailBounced[1].Recipient == "bounced2@bounced.com");
			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.MailDeferred[0].Recipient == "deferred@deferred.com" && x.MailDeferred[1].Recipient == "deferred2@deferred.com");
		}

		[Test]
		public void Execute_WhenExecuted_ShouldSendAllCreativeActions()
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
				                             }, "log");

			DroneActions.Store(new ClickAction
								   {
									   ContactId = "contacts/1",
									   CreativeId = "creative/1",
									   Date = DateTime.UtcNow
								   }
				);

			DroneActions.Store(new UnsubscribeRequest
								   {
									   ContactId = "contacts/2",
									   CreativeId = "creative/2",
									   Date = DateTime.UtcNow
								   }
				);


			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.ClickActions[0].ContactId == "contacts/1");
			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x => x.UnsubscribeRequests[0].ContactId == "contacts/2");
		}

		[Test]
		public void Execute_WhenSendingToMasterSuccessful_ShouldDeleteAllData()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.Identifier = "drone1";
				x.BaseUrl = "http://base.com";
				x.StoreHostname = DefaultHostUrl;
			});

			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);


			Api.ListenToApiCall<ServiceEndpoints.Drones.SendStateSnapshot>();

			DroneActions.Store(new ClickAction
								   {
									   ContactId = "contacts/1",
									   CreativeId = "creative/1",
									   Date = DateTime.UtcNow
								   }
				);

			DroneActions.Store(new UnsubscribeRequest
								   {
									   ContactId = "contacts/2",
									   CreativeId = "creative/2",
									   Date = DateTime.UtcNow
								   }
				);

			DroneActions.StoreCollection(new[]
				                             {
												 new MailSent { Recipient = "sent@sent.com" },
												 new MailSent { Recipient = "sent2@sent.com" }
				                             });

			DroneActions.StoreCollection(new[]
				                             {
												 new MailBounced { Recipient = "bounced@bounced.com" },
												 new MailBounced { Recipient = "bounced2@bounced.com" }
				                             });

			DroneActions.StoreCollection(new[]
				                             {
					                             new MailDeferred {Recipient = "deferred@deferred.com"},
					                             new MailDeferred {Recipient = "deferred2@deferred.com"}
				                             });

			DroneActions.StoreCollection(new[]
				                             {
					                             new MailLogEntry { msg = "message 1" },
					                             new MailLogEntry { msg = "message 2" },
				                             }, "log");

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			DroneActions.WaitForEmptyListOf<MailLogEntry>();
			DroneActions.WaitForEmptyListOf<ClickAction>();
			DroneActions.WaitForEmptyListOf<UnsubscribeRequest>();
			DroneActions.WaitForEmptyListOf<MailSent>();
			DroneActions.WaitForEmptyListOf<MailBounced>();
			DroneActions.WaitForEmptyListOf<MailDeferred>();
		}
	}
}
