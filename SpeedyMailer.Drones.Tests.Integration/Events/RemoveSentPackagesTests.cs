using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class RemoveSentPackagesTests : IntegrationTestBase
	{
		public RemoveSentPackagesTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Inspect_WhenAnPackageWasSent_ShouldRemoveItFromTheStore()
		{
			var package = new CreativePackage
				{
					To = "david@david.com",
					Group = "$default$"
				};

			DroneActions.Store(package);

			FireEvent<RemoveSentPackages, AggregatedMailSent>(x => x.MailEvents = new List<MailSent>
				{
					new MailSent
						{
							Recipient = "david@david.com"
						}
				});

			var result = DroneActions.FindAll<CreativePackage>();

			result.Should().BeEmpty();
		}
	}
}
