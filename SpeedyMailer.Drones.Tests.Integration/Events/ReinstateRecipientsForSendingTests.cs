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
	public class ReinstateRecipientsForSendingTests : IntegrationTestBase
	{
		public ReinstateRecipientsForSendingTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Happend_WhenBouncedMailIsClassifiedAsBlocked_ShouldCreateAPackageUsingThatEmail()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			FireEvent<ReinstateRecipientsForSending,
				AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
					                                           {
						                                           new MailBounced
							                                           {
								                                           Message = "bounced that blocked",
								                                           DomainGroup = "gmail",
								                                           Recipient = "david@david.com"
							                                           }
					                                           });

			DroneActions.WaitForDocumentToExist<CreativePackage>();

			var result = DroneActions.FindSingle<CreativePackage>();

			result.To.Should().Be("david@davi.com");
		}
	}
}
