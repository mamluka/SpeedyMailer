using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class ResendTryAgainProtectedContactsTests : IntegrationTestBase
	{
		public ResendTryAgainProtectedContactsTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Inspect_WhenThereAreTryAgainEmails_ShouldResendThem()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			DroneActions.StoreCollection(new[]
				{
					CreatePackage("david@david.com"),
					CreatePackage("david@gmail.com")
				});

			FireEvent<ResendTryAgainProtectedContacts, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced { Classification = new MailClassfication { Type = Classification.TryAgain},Recipient = "david@david.com"},
					new MailBounced { Classification = new MailClassfication { Type = Classification.HardBounce},Recipient = "david@gmail.com"}
				});

			Email.AssertEmailsSentTo(new[] { "david@david.com" });
			Email.AssertEmailNotSentTo(new[] { "david@gmail.com" });
		}

		[Test]
		public void Inspect_WhenThereAreNoTryAgainEmails_ShouldDoNothing()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			DroneActions.StoreCollection(new[]
				{
					CreatePackage("david@david.com"),
					CreatePackage("david@gmail.com")
				});

			FireEvent<ResendTryAgainProtectedContacts, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>());

			Email.AssertEmailNotSentTo(new[] { "david@gmail.com", "david@david.com" });
		}

		private static CreativePackage CreatePackage(string email)
		{
			return new CreativePackage
			{
				HtmlBody = "body",
				Group = "cool",
				Subject = "subject",
				To = email,
				FromAddressDomainPrefix = "david",
				Interval = 10,
				FromName = "sales",
				CreativeId = "creative/1",
				TouchTime = DateTime.UtcNow

			};
		}
	}
}
