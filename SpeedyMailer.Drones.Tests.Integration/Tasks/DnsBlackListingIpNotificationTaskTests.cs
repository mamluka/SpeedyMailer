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
		public DnsBlackListingIpNotificationTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenDNSBLArePresentInTheJson_ShouldQueryThemForTheCurrentIp()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.Ip = "69.85.89.203";
				x.StoreHostname = DefaultHostUrl;
			});

			var task = new BlacklistingDnsCheckTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<BlacklistingIpInformation>();

			var result = DroneActions.FindSingle<BlacklistingIpInformation>();

			result.Answers.Should().NotBeEmpty();
		}
	}

	public class ListedOnDnsnl
	{
		public string ServiceName { get; set; }
	}
}
