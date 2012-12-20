using System;
using System.Collections.Generic;
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
					x.LastUpdate = DateTime.UtcNow.ToLongTimeString();
					x.Domain = "example.com";
					x.IpReputation = new IpReputation { BlockingHistory = new Dictionary<string, List<DateTime>> { { "gmail", new List<DateTime> { new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) } } } };
				});

			Store.WaitForEntitiesToExist<Drone>();
			var result = Store.Query<Drone>().First();

			result.BaseUrl.Should().Be("baseurl");
			result.Id.Should().Be("droneip");
			result.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddSeconds(-30));
			result.Domain.Should().Be("example.com");
			result.IpReputation.BlockingHistory["gmail"].Should().Contain(x => x == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void SendStateSnapshot_WhenCalledWithADroneIdentifier_ShouldRegisterTheDroneInTheStore()
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

																		x.MailDeferred = new List<MailDeferred>
																			                {
																				                new MailDeferred { Message = "mail deferred"}
																			                };

																		x.MailSent = new List<MailSent>
																			                {
																				                new MailSent { Recipient = "sent@sent.com"}
																			                };

																		x.RawLogs = new List<ReducedMailLogEntry>
																			            {
																				            new ReducedMailLogEntry
																					            {
																						            Message = "log message"
																					            }
																			            };

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
																	});

			Store.WaitForEntitiesToExist<DroneStateSnapshoot>();
			var result = Store.Query<DroneStateSnapshoot>().First();

			result.Drone.Id.Should().Be("identifier");
			result.Drone.BaseUrl.Should().Be("baseurl.com");
			result.Drone.Domain.Should().Be("example.com");

			result.RawLogs.Should().OnlyContain(x => x.Message == "log message");
			result.MailSent.Should().OnlyContain(x => x.Recipient == "sent@sent.com");
			result.MailBounced.Should().OnlyContain(x => x.Message == "mail bounced");
			result.MailDeferred.Should().OnlyContain(x => x.Message == "mail deferred");
			result.ClickActions.Should().OnlyContain(x => x.ContactId == "contacts/1");
			result.UnsubscribeRequests.Should().OnlyContain(x => x.ContactId == "contacts/2");
		}

		[Test]
		public void GetDnsblData_WhenCalled_ShouldReturnTheListOfDnsblServices()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			var api = DroneResolve<Api>();

			var result = api.Call<ServiceEndpoints.Drones.GetDnsblData, List<Dnsbl>>();

			result.Should().HaveCount(27);
			result[0].Dns.Should().Be("virbl.Dnsbl.bit.nl");
			result[0].Name.Should().Be("VIRBL");
			result[0].Type.Should().Be(DnsnlType.Ip);
		}
	}
}
