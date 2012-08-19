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
		public CreativePackage Package { get; set; }

		public SendCreativePackageCommand(EmailingSettings emailingSettings)
		{
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
				var emailFile = JsonConvert.SerializeObject(email,
															Formatting.Indented,
															new JsonSerializerSettings
																{
																	NullValueHandling = NullValueHandling.Ignore
																});

				using (var writer = new StreamWriter(Path.Combine(_emailingSettings.WritingEmailsToDiskPath, "email" + Guid.NewGuid() + ".persist")))
				{
					writer.Write(emailFile);
					Trace.WriteLine("Email written to disk:\n\r" + emailFile);
					writer.Flush();
				}
			}
			else
			{
				var client = new SmtpClient();
				client.Send(email);
			}
		}
	}
}