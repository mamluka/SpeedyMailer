using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using Newtonsoft.Json;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones.Commands
{
	public class SendCreativePackageCommand : Command
	{
		private readonly EmailingSettings _emailingSettings;
		private DroneSettings _droneSettings;
		public CreativePackage Package { get; set; }

		public SendCreativePackageCommand(EmailingSettings emailingSettings,DroneSettings droneSettings)
		{
			_droneSettings = droneSettings;
			_emailingSettings = emailingSettings;
		}

		public override void Execute()
		{
			var email = new MailMessage();
			email.To.Add(Package.To);
			email.Body = Package.Body;
			email.Subject = Package.Subject;

			if (!string.IsNullOrEmpty(_emailingSettings.WritingEmailsToDiskPath))
			{
				var tmpEmailFile = SerializeObject(email);
				var fakeEmailFile = JsonConvert.DeserializeObject<FakeEmailMessage>(tmpEmailFile);
				fakeEmailFile.DroneId = _droneSettings.Identifier;

				var emailFile = SerializeObject(fakeEmailFile);

				using (var writer = new StreamWriter(Path.Combine(_emailingSettings.WritingEmailsToDiskPath, "email" + Guid.NewGuid() + ".persist")))
				{
					writer.Write(emailFile);
					writer.Flush();
				}
			}
			else
			{
				var client = new SmtpClient();
				client.Send(email);
			}
		}

		private static string SerializeObject(object email)
		{
			return JsonConvert.SerializeObject(email,
			                                   Formatting.Indented,
			                                   new JsonSerializerSettings
				                                   {
					                                   NullValueHandling = NullValueHandling.Ignore
				                                   });
		}
	}
}