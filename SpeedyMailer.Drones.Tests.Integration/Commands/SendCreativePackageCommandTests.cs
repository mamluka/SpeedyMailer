using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime;
using NUnit.Framework;
using Newtonsoft.Json;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Emails;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class SendCreativePackageCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenToldToWriteToDisk_ShouldWriteTheEmailToDisk()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);

			var creativePackage = new CreativePackage
									{
										Body = "test body",
										Subject = "test subject",
										To = "test@test"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x => x.Package = creativePackage);

			AssertEmailSent(x => x.To.Any(address=> address.Address == "test@test"));
			AssertEmailSent(x => x.Body == "test body");
			AssertEmailSent(x => x.Subject == "test subject");
		}
	}
}
