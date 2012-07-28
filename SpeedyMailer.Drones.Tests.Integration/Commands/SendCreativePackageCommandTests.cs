using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class SendCreativePackageCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenToldToWriteToDisk_ShouldWriteTheActualEmailToDisk()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);

			var creativePackage = new CreativePackage
									{
										Body = "test body",
										Subject = "test subject",
										To = "test@test"
									};

			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x => x.Package = creativePackage);

			AssertEmailSent(x => x.To.Contains(new MailAddress("test@test")));


		}

		private void AssertEmailSent(Func<MailMessage, bool> func)
		{
			var files = Directory.GetFiles(AssemblyDirectory).Where(x =>
																		{
																			var filename = Path.GetFileName(x);
																			if (string.IsNullOrEmpty(filename))
																				return false;

																			return filename.StartsWith("email") && filename.EndsWith(".persist");
																		}).ToArray();
			if (!files.Any())
				Assert.Fail("No emails were found");

			var email = JsonConvert.DeserializeObject<MailMessage>(File.ReadAllText(files.First()));
			Assert.That(func(email), Is.True);
		}
	}

	public class EmailAssertion
	{
		public string To { get; set; }
	}
}
