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
		{
		}

		[Test]
		public void Execute_WhenGivenAMessageThatMatchesHardBounce_ShouldReturnHardBounce()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message =
				"host mta6.am0.yahoodns.net[98.139.54.60] said: 554 delivery error: dd Sorry your message to bigluke89@yahoo.com cannot be delivered. This account has been disabled or discontinued [#102]. - mta1221.mail.ac4.yahoo.com (in reply to end of DATA command)";

			StoreClassficationRules(new[]
				{
					new HeuristicRule {Condition = "account.+?disabled", Type = Classification.HardBounce},
					new HeuristicRule {Condition = "doesn't have.+?account", Type = Classification.HardBounce}
				});

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.HardBounce);
		}

		[Test]
		public void Execute_WhenGivenAMessageThatMatchesTempIpBlocked_ShouldReturnnBlocked()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-03.mx.aol.com[205.188.156.193] refused to talk to me: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			StoreClassficationRules(new[] { new HeuristicRule { Condition = "DYN:T1", Type = Classification.TempBlock, Data = new HeuristicData { TimeSpan = TimeSpan.FromHours(2) } } });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.TempBlock);
		}

		[Test]
		public void Execute_WhenGivenAMessageThatMatchesIpBlockeing_ShouldReturnnIpBlocking()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-03.mx.aol.com[205.188.156.193] blocking for ever: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			StoreClassficationRules(new[] { new HeuristicRule { Condition = "blocking for ever", Type = Classification.IpBlocking } });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.IpBlocking);
		}

		[Test]
		public void Execute_WhenGivenAMessageThatMatchesContentBlockeing_ShouldReturnnContentBlocking()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-03.mx.aol.com[205.188.156.193] spam content: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			StoreClassficationRules(new[] { new HeuristicRule { Condition = "spam content", Type = Classification.ContentBlocking } });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.ContentBlocking);
		}
		
		[Test]
		public void Execute_WhenGivenAMessageThatMatchesTryAgain_ShouldReturnnTryAgain()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-03.mx.aol.com[205.188.156.193] requested action aborted: try again";

			StoreClassficationRules(new[] { new HeuristicRule { Condition = "try again", Type = Classification.TryAgain } });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.TryAgain);
		}
		
		[Test]
		public void Execute_WhenConditionIsGiven_ShouldNotBeCaseSensitive()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message =
				"host mta6.am0.yahoodns.net[98.139.54.60] said: 554 delivery error: dd Sorry your message to bigluke89@yahoo.com cannot be delivered. This Account has been Disabled or discontinued [#102]. - mta1221.mail.ac4.yahoo.com (in reply to end of DATA command)";

			StoreClassficationRules(new[]
				{
					new HeuristicRule {Condition = "account.+?disabled", Type = Classification.HardBounce},
					new HeuristicRule {Condition = "doesn't have.+?account", Type = Classification.HardBounce}
				});

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.HardBounce);
		}

		[Test]
		public void Execute_WhenNoClassificationRulesPresent_ShouldReturnNotClassified()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			const string message = "host mailin-01.mx.aol.com[205.188.159.42] said: 550 5.1.1 <acrandell@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command)";

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, MailClassfication>(x => x.Message = message);

			result.Type.Should().Be(Classification.NotClassified);
		}

		private void StoreClassficationRules(IEnumerable<HeuristicRule> rules)
		{
			DroneActions.Store(new DeliverabilityClassificationRules
				{
					Rules = rules.ToList()
				});
		}
	}
}
