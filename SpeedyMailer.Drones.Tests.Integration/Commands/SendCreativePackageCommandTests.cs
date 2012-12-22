using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
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
										HtmlBody = "test body",
										Subject = "test subject",
										To = "test@test",
										CreativeId = "creative/1"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x =>
																		{
																			x.Package = creativePackage;
																			x.FromName = "david";
																			x.FromAddressDomainPrefix = "sales";
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
										HtmlBody = "test body",
										Subject = "test subject",
										To = "test@test",
                                        CreativeId = "creative/1"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x =>
																		{
																			x.Package = creativePackage;
																			x.FromName = "david";
																			x.FromAddressDomainPrefix = "sales";
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
										CreativeId = "creative/1"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x =>
																		{
																			x.Package = creativePackage;
																			x.FromName = "david";
																			x.FromAddressDomainPrefix = "sales";
																		});

			Email.AssertEmailSent(x => x.From.Address == "sales@example.com" && x.From.DisplayName == "david");
		}

		[Test]
		public void Execute_WhenPackageIsNull_ShouldDoNothing()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x => x.Package = null);

			Email.AssertEmailNotSent(10);
		}
	}
}
