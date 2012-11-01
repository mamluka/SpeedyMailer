using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime;
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
										Body = "test body",
										Subject = "test subject",
										To = "test@test"
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
		public void Execute_WhenGivenFromInformation_ShouldSendTheEmailWithThatFromAddress()
		{
			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackage = new CreativePackage
									{
										Body = "test body",
										Subject = "test subject",
										To = "test@test"
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
