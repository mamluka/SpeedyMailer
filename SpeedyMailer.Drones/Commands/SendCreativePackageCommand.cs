using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using NLog;
using Nancy.Extensions;
using Newtonsoft.Json;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Commands
{
	public class SendCreativePackageCommand : Command
	{
		private readonly EmailingSettings _emailingSettings;
		private readonly DroneSettings _droneSettings;
		private readonly Logger _logger;

		public CreativePackage Package { get; set; }
		public string FromName { get; set; }
		public string FromAddressDomainPrefix { get; set; }

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
			email.Body = Package.TextBody;

			if (!string.IsNullOrEmpty(Package.HtmlBody))
				email.AlternateViews.Add(CreateHtmlView());

			email.Subject = Package.Subject;
			email.From = new MailAddress(FromAddressDomainPrefix + "@" + _emailingSettings.MailingDomain, FromName);
			email.IsBodyHtml = false;
			email.Headers.Add("Speedy-Creative-Id", Package.CreativeId);

			if (!string.IsNullOrEmpty(_emailingSettings.WritingEmailsToDiskPath))
			{
				if (_emailingSettings.WritingEmailsToDiskPath != "dev/null")
				{
					WriteEmailToDisk(email);
				}
			}
			else
			{
				var client = new SmtpClient(_emailingSettings.SmtpHost);
				client.Send(email);
			}

			_logger.Info("Drone: {0} sent email to: {1}, with subject {2}, email body was: {3}", _droneSettings.Identifier, Package.To, Package.Subject, Package.HtmlBody);
		}

		private AlternateView CreateHtmlView()
		{
			var contentType = new System.Net.Mime.ContentType("text/html");
			return AlternateView.CreateAlternateViewFromString(Package.HtmlBody, contentType);
		}

		private void WriteEmailToDisk(MailMessage email)
		{

			var htmlView = email.AlternateViews.Any() ? new StreamReader(email.AlternateViews[0].ContentStream).ReadToEnd() : "";
			email.AlternateViews.Clear();


			var tmpEmailFile = SerializeObject(email);
			var fakeEmailFile = JsonConvert.DeserializeObject<FakeEmailMessage>(tmpEmailFile);
			fakeEmailFile.DroneId = _droneSettings.Identifier;
			fakeEmailFile.SendTime = DateTime.UtcNow;
			fakeEmailFile.DeliveryDate = DateTime.UtcNow;
			fakeEmailFile.TestHeaders = email.Headers.AllKeys.ToDictionary(x => x, x => email.Headers[x]);

			fakeEmailFile.AlternateViews.Add(htmlView);

			var emailFile = SerializeObject(fakeEmailFile);

			using (var writer = new StreamWriter(CreateEmailFilePath()))
			{
				writer.Write(emailFile);
				writer.Flush();
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