using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class DnsBlackListingIpNotificationTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenDNSBLArePresentInTheJson_ShouldQueryThemForTheCurrentIp()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.Identifier = "drone1";
				x.BaseUrl = "http://192.168.1.1:2589";
				x.Domain = "domain.com";
				x.Ip = "127.0.0.1";
			});

			ListenToEvent<ListedOnDnsnl>();

			var task = new DnsBlackListingIpNotificationTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<ListedOnDnsnl>(x => x.ServiceName.Should().Be("x"));
		}
	}

	public class ListedOnDnsnl
	{
		public string ServiceName { get; set; }
	}
}
