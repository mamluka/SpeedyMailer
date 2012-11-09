using System;
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

			DroneActions.Store(new HardBounceHeuristics
								   {
									   HardBounceRules = new[]
						                                     {
							                                     "account.+?disabled",
							                                     "doesn't have.+?account",
						                                     }
								   });

			var result = DroneActions.ExecuteCommand<ClassifyBouncedMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.HardBounce);
		}
		
		[Test]
		public void Execute_WhenGivenAMessageThatDoesnt_ShouldReturnNotClassified()
		{
			const string message = "host mailin-01.mx.aol.com[205.188.159.42] said: 550 5.1.1 <acrandell@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command)";

			DroneActions.Store(new HardBounceHeuristics
								   {
									   HardBounceRules = new[]
						                                     {
							                                     "account.+?disabled",
							                                     "doesn't have.+?account",
						                                     }
								   });

			var result = DroneActions.ExecuteCommand<ClassifyBouncedMailCommand, BounceType>(x => x.Message = message);

			result.Should().Be(BounceType.NotClassified);
		}
	}
}
