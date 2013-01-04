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
	public class StoreUnclassifiedMailEventsTests : IntegrationTestBase
	{
		public StoreUnclassifiedMailEventsTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Inspect_WhenBouncedMailsAreNotClassified_ShouldStoreThem()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			FireEvent<StoreUnclassifiedMailEvents, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							Message = "unclassified",
							CreativeId = "creative/1",
							DomainGroup = "gmail",
							Recipient = "david@gmail.com",
							Type = new MailClassfication {Classification = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
				});

			var result = DroneActions.FindSingle<UnclassfiedMailEvent>();

			result.CreativeId.Should().Be("creative/1");
			result.DomainGroup.Should().Be("gmail");
			result.Message.Should().Be("unclassified");
			result.Recipient.Should().Be("david@gmail.com");
		}

		[Test]
		public void Inspect_WhenMailsAreNotClassified_ShouldUpdateTheRelevantCreativePackageTouchDateToNow()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new CreativePackage
				{
					To = "david@gmail.com",
					TouchTime = DateTime.UtcNow.AddMinutes(-30)
				});

			FireEvent<StoreUnclassifiedMailEvents, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							Message = "unclassified",
							CreativeId = "creative/1",
							DomainGroup = "gmail",
							Recipient = "david@gmail.com",
							Type = new MailClassfication {Classification = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
				});

			var result = DroneActions.FindSingle<CreativePackage>();

			result.TouchTime.Should().BeAfter(DateTime.UtcNow - TimeSpan.FromSeconds(10));
		}
	}
}
