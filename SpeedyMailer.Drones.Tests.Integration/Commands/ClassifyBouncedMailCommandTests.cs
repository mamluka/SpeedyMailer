using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
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
			const string message = "host mta6.am0.yahoodns.net[98.139.54.60] said: 554 delivery error: dd Sorry your message to bigluke89@yahoo.com cannot be delivered. This account has been disabled or discontinued [#102]. - mta1221.mail.ac4.yahoo.com (in reply to end of DATA command)";

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
								   {
									   HardBounceRules = new List<string>
						                                     {
							                                     "account.+?disabled",
							                                     "doesn't have.+?account",
						                                     }
								   });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.HardBounce);
		}
		
		[Test]
		public void Execute_WhenGivenAMessageThatDoesntMatchHardBounce_ShouldReturnNotClassified()
		{
			const string message = "host mailin-01.mx.aol.com[205.188.159.42] said: 550 5.1.1 <acrandell@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command)";

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
								   {
									   HardBounceRules = new List<string>
						                                     {
							                                     "account.+?disabled",
							                                     "doesn't have.+?account",
						                                     }
								   });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.NotClassified);
		}
		
		[Test]
		public void Execute_WhenGivenAMessageThatMatchesIpBlocked_ShouldReturnHardBounce()
		{
			const string message = "host mailin-03.mx.aol.com[205.188.156.193] refused to talk to me: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
								   {
									   IpBlockingRules = new List<string>
						                                     {
							                                     "DYN:T1",
						                                     }
								   });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.IpBlocked);
		}
		
		[Test]
		public void Execute_WhenGivenAMessageThatDoesntMatchIpBlocked_ShouldReturnNotClassified()
		{
			const string message = "host mailin-03.mx.aol.com[205.188.156.193] refused to talk to me: 421 4.7.1 : (DYN:T1) http://postmaster.info.aol.com/errors/421dynt1.html";

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
								   {
									   IpBlockingRules = new List<string>
						                                     {
							                                     "Resources temporarily unavailable",
						                                     }
								   });

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.NotClassified);
		}
		
		[Test]
		public void Execute_WhenNoClassificationRulesPresent_ShouldReturnNotClassified()
		{
			const string message = "host mailin-01.mx.aol.com[205.188.159.42] said: 550 5.1.1 <acrandell@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command)";

			var result = DroneActions.ExecuteCommand<ClassifyNonDeliveredMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.NotClassified);
		}
	}
}
