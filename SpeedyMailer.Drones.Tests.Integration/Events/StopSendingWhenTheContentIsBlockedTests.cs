using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class StopSendingWhenTheContentIsBlockedTests:IntegrationTestBase
	{
		[Test]
		public void Inspect_WhenTheBlockTypeIsIpBlocking_ShouldStopSendingForAllOfTheDomains()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.StoreCollection(new[]
				{
					CreatePackage("david@me.com"),
					CreatePackage("cookie@me.com"),
					CreatePackage("david@notme.com")
				});

			FireEvent<StopSendingWhenTheContentIsBlocked, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							Domain = "me.com",
							Classification = new MailClassfication {Type = Classification.ContentBlocking}
						}
				});

			var result = DroneActions.FindAll<CreativePackage>();

			result.Should().Contain(x => x.Processed && x.To == "david@me.com");
			result.Should().Contain(x => x.Processed && x.To == "cookie@me.com");
			result.Should().Contain(x => x.Processed == false && x.To == "david@notme.com");
		}

		private CreativePackage CreatePackage(string to)
		{
			return new CreativePackage
			{
				To = to,
				Group = "gmail"
			};
		}
	}
}
