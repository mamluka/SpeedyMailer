using System.Linq;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class SendCreativePackageCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenToldToWriteToDisk_ShouldWriteTheEmailToDisk()
		{
			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackage = new CreativePackage
									{
										TextBody = "test body",
										Subject = "test subject",
										To = "test@test",
										CreativeId = "creative/1",
										FromName = "david",
										FromAddressDomainPrefix = "sales"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x =>
																		{
																			x.Package = creativePackage;
																		});

			Email.AssertEmailSent(x => x.To.Any(address => address.Address == "test@test"));
			Email.AssertEmailSent(x => x.Body == "test body");
			Email.AssertEmailSent(x => x.Subject == "test subject");
		}
        
        [Test]
		public void Execute_WhenSendingTheCreative_ShouldSetCreativeIdHeader()
		{
			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackage = new CreativePackage
									{
										TextBody = "test body",
										Subject = "test subject",
										To = "test@test",
                                        CreativeId = "creative/1",
										FromName = "david",
										FromAddressDomainPrefix = "sales"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x =>
																		{
																			x.Package = creativePackage;
																		});

			Email.AssertEmailSent(x => x.To.Any(address => address.Address == "test@test"));
			Email.AssertEmailSent(x => x.Body == "test body");
			Email.AssertEmailSent(x => x.Subject == "test subject");
            Email.AssertEmailSent(x => x.TestHeaders["Speedy-Creative-Id"] == "creative/1");
		}

		[Test]
		public void Execute_WhenGivenFromInformation_ShouldSendTheEmailWithThatFromAddress()
		{
			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackage = new CreativePackage
									{
										HtmlBody = "test body",
										Subject = "test subject",
										To = "test@test",
										CreativeId = "creative/1",
										FromName = "david",
										FromAddressDomainPrefix = "sales"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x =>
																		{
																			x.Package = creativePackage;
																		});

			Email.AssertEmailSent(x => x.From.Address == "sales@example.com" && x.From.DisplayName == "david");
		}

		[Test]
		public void Execute_WhenPackageIsNull_ShouldDoNothing()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x => x.Package = null);

			Email.AssertEmailNotSentTo(10);
		}
	}
}
