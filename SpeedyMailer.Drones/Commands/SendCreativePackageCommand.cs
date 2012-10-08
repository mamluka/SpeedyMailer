using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Threading;
using Newtonsoft.Json;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones.Commands
{
	public class SendCreativePackageCommand : Command
	{
		private readonly EmailingSettings _emailingSettings;
		private readonly DroneSettings _droneSettings;
		public CreativePackage Package { get; set; }

		public SendCreativePackageCommand(EmailingSettings emailingSettings, DroneSettings droneSettings)
		{
			_droneSettings = droneSettings;
			_emailingSettings = emailingSettings;
		}

		public override void Execute()
		{
			if (Package == null)
				return;

			var email = new MailMessage();
			email.To.Add(Package.To);
			email.Body = Package.Body;
			email.Subject = Package.Subject;

			if (!string.IsNullOrEmpty(_emailingSettings.WritingEmailsToDiskPath))
			{
				var tmpEmailFile = SerializeObject(email);
				var fakeEmailFile = JsonConvert.DeserializeObject<FakeEmailMessage>(tmpEmailFile);
				fakeEmailFile.DroneId = _droneSettings.Identifier;
				fakeEmailFile.DeliveryDate = DateTime.UtcNow;

				var emailFile = SerializeObject(fakeEmailFile);

				using (var writer = new StreamWriter(CreateEmailFilePath()))
				{
					writer.Write(emailFile);
					Trace.WriteLine("Email sent to :" + Package.To);
					writer.Flush();
				}
			}
			else
			{
				var client = new SmtpClient();
				client.Send(email);
			}
		}

		private string CreateEmailFilePath()
		{
			string filename = string.Format("email_{0}_{1}.persist", _droneSettings.Identifier, Guid.NewGuid());
			return Path.Combine(_emailingSettings.WritingEmailsToDiskPath, filename);
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