using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class ClassifyBouncedMailCommandTests : IntegrationTestBase
	{
		public ClassifyBouncedMailCommandTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenGivenAMessageThatMatchesHardBounce_ShouldReturnHardBounce()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mta6.am0.yahoodns.net[98.139.54.60] said: 554 delivery error: dd Sorry your message to bigluke89@yahoo.com cannot be delivered. This account has been disabled or discontinued [#102]. - mta1221.mail.ac4.yahoo.com (in reply to end of DATA command)";

			StoreClassficationRules(new[]
				                        {
					                        "account.+?disabled",
					                        "doesn't have.+?account",
				                        });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.BounceType.Should().Be(BounceType.HardBounce);
		}
		
		[Test]
		public void Execute_WhenGivenAMessageThatMatchesHardBounceWithConditionInDifferentCase_ShouldReturnHardBounce()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mta6.am0.yahoodns.net[98.139.54.60] said: 554 delivery error: dd Sorry your message to bigluke89@yahoo.com cannot be delivered. This account has been disabled or discontinued [#102]. - mta1221.mail.ac4.yahoo.com (in reply to end of DATA command)";

			StoreClassficationRules(new[]
				                        {
					                        "Account.+?Disabled",
					                        "doesn't have.+?account",
				                        });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.BounceType.Should().Be(BounceType.HardBounce);
		}

		[Test]
		public void Execute_WhenGivenAMessageThatDoesntMatchHardBounce_ShouldReturnNotClassified()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-01.mx.aol.com[205.188.159.42] said: 550 5.1.1 <acrandell@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command)";

			StoreClassficationRules(new[]
				                        {
					                        "account.+?disabled",
					                        "doesn't have.+?account",
				                        });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.BounceType.Should().Be(BounceType.NotClassified);
		}

		[Test]
		public void Execute_WhenGivenAMessageThatMatchesIpBlocked_ShouldReturnnBlocked()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-03.mx.aol.com[205.188.156.193] refused to talk to me: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			StoreClassficationRules(null, new[] { new HeuristicRule { Condition = "DYN:T1", TimeSpan = TimeSpan.FromHours(2) } });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.BounceType.Should().Be(BounceType.Blocked);
		}

		[Test]
		public void Execute_WhenGivenAMessageThatDoesntMatchIpBlocked_ShouldReturnNotClassified()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-03.mx.aol.com[205.188.156.193] refused to talk to me: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			StoreClassficationRules(null, new[] { new HeuristicRule { Condition = "Resources temporarily unavailable", TimeSpan = TimeSpan.FromHours(2) } });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.BounceType.Should().Be(BounceType.NotClassified);
		}

		[Test]
		public void Execute_WhenNoClassificationRulesPresent_ShouldReturnNotClassified()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-01.mx.aol.com[205.188.159.42] said: 550 5.1.1 <acrandell@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command)";

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.BounceType.Should().Be(BounceType.NotClassified);
		}

		private void StoreClassficationRules(IEnumerable<string> hardBounceRules, IEnumerable<HeuristicRule> blockingRules = null)
		{
			DroneActions.Store(new DeliverabilityClassificationRules
								   {
									   HardBounceRules = hardBounceRules.EmptyIfNull().ToList(),
									   BlockingRules = blockingRules.EmptyIfNull().ToList()
								   });
		}
	}
}
