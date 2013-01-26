using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class DronesModuleTests : IntegrationTestBase
	{
		[Test]
		public void RegisterDrone_WhenCalledWithADroneIdentifier_ShouldRegisterTheDroneInTheStore()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			var api = DroneResolve<Api>();

			api.Call<ServiceEndpoints.Drones.RegisterDrone>(x =>
				{
					x.Identifier = "droneip";
					x.BaseUrl = "baseurl";
					x.Domain = "example.com";
					x.IpReputation = new IpReputation { BlockingHistory = new Dictionary<string, List<DateTime>> { { "gmail", new List<DateTime> { new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) } } } };
					x.Exceptions = new List<DroneException> { new DroneException { Component = "c", Time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Message = "message", Exception = "exception" } };
				});

			Store.WaitForEntitiesToExist<Drone>();
			var result = Store.Query<Drone>().First();

			result.BaseUrl.Should().Be("baseurl");
			result.Id.Should().Be("droneip");
			result.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddSeconds(-30));
			result.Domain.Should().Be("example.com");
			result.IpReputation.BlockingHistory["gmail"].Should().Contain(x => x == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			result.Exceptions.Should()
				  .Contain(x =>
						   x.Component == "c" &&
						   x.Exception == "exception" &&
						   x.Message == "message" &&
						   x.Time == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString());
		}

		[Test]
		public void SendStateSnapshot_WhenGotASnapStop_ShouldStoreIt()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			var api = DroneResolve<Api>();

			api.Call<ServiceEndpoints.Drones.SendStateSnapshot>(x =>
																	{
																		x.Drone = new Drone
																					  {
																						  BaseUrl = "baseurl.com",
																						  Domain = "example.com",
																						  Id = "identifier"
																					  };
																		x.MailBounced = new List<MailBounced>
																			                {
																				                new MailBounced { Message = "mail bounced"}
																			                };

																		x.MailSent = new List<MailSent>
																			                {
																				                new MailSent { Recipient = "sent@sent.com"}
																			                };

																		x.RawLogs = new List<string> { "log message" };

																		x.ClickActions = new List<ClickAction>
																			                 {
																				                 new ClickAction
																					                 {
																						                 ContactId = "contacts/1",
																										 CreativeId = "creative/1"
																					                 }
																			                 };

																		x.UnsubscribeRequests = new List<UnsubscribeRequest>
																			                        {
																				                        new UnsubscribeRequest
																					                        {
																						                        ContactId = "contacts/2",
																												CreativeId = "creative/2"
																					                        }
																			                        };
																		x.Unclassified = new List<UnclassfiedMailEvent>
																			{
																				new UnclassfiedMailEvent
																					{
																						Message = "unclassified"
																					}
																			};
																	});

			Store.WaitForEntitiesToExist<DroneStateSnapshoot>();
			var result = Store.Query<DroneStateSnapshoot>().First();

			result.Drone.Id.Should().Be("identifier");
			result.Drone.BaseUrl.Should().Be("baseurl.com");
			result.Drone.Domain.Should().Be("example.com");

			result.MailSent.Should().OnlyContain(x => x.Recipient == "sent@sent.com");
			result.MailBounced.Should().OnlyContain(x => x.Message == "mail bounced");
			result.ClickActions.Should().OnlyContain(x => x.ContactId == "contacts/1");
			result.UnsubscribeRequests.Should().OnlyContain(x => x.ContactId == "contacts/2");
			result.Unclassified.Should().OnlyContain(x => x.Message == "unclassified");
		}

		[Test]
		public void SendStateSnapshot_WhenGotARawLogs_ShouldStoreThemInAFile()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			var api = DroneResolve<Api>();

			api.Call<ServiceEndpoints.Drones.SendStateSnapshot>(x =>
																	{
																		x.Drone = new Drone { Id = "drone1.com" };
																		x.RawLogs = new List<string> { "log message" };
																	});

			var result = ReadLogFile("drone1.com.txt");

			result.Should().Contain("log message");
		}

		private string ReadLogFile(string droneId)
		{
			using (var reader = new StreamReader(@"logs\drones\" + droneId))
			{
				return reader.ReadToEnd();
			}
		}

		[Test]
		public void Get_WhenCalled_ShouldReturnDrones()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			Store.Store(new Drone
				{
					Id = "1",
				});

			Store.WaitForEntitiesToExist<Drone>();

			var api = DroneResolve<Api>();

			var result = api.Call<ServiceEndpoints.Drones.Get, List<Drone>>();

			result.Should().HaveCount(1);
			result.Should().Contain(x => x.Id == "1");
		}
	}
}
