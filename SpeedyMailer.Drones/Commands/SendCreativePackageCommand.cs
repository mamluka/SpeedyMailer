using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Threading;
using NLog;
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
		private Logger _logger;
		public CreativePackage Package { get; set; }

		public SendCreativePackageCommand(Logger logger, EmailingSettings emailingSettings, DroneSettings droneSettings)
		{
			_logger = logger;
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
					writer.Flush();
				}
			}
			else
			{
				var client = new SmtpClient();
				client.Send(email);
			}

			_logger.Info("Email sent to: {0}, with subject {1}, email body was: {2}", Package.To, Package.Subject, Package.Body);
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