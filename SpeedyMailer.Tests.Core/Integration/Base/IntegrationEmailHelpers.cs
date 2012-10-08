using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationEmailHelpers
	{
		public void AssertEmailSent(Func<FakeEmailMessage, bool> func, int waitFor = 30)
		{
			var email = GetEmailFromDisk(waitFor);
			Assert.That(func(email));
		}

		public void AssertEmailSent(Action<FakeEmailMessage> action, int waitFor = 30)
		{
			var email = GetEmailFromDisk(waitFor);
			action(email);
		}

		public void AssertEmailsSentTo(IEnumerable<string> contacts, int waitFor = 30)
		{
			var files = WaitForEmailFiles(waitFor);

			if (!files.Any())
				Assert.Fail("No emails were found");

			var emails = files.Select(x => JsonConvert.DeserializeObject<FakeEmailMessage>(File.ReadAllText(x)));

			emails.Should().OnlyContain(x => x.To.Any(p => contacts.Contains(p.Address)));
		}

		private FakeEmailMessage GetEmailFromDisk(int waitFor)
		{
			var files = WaitForEmailFiles(waitFor);

			if (!files.Any())
				Assert.Fail("No emails were found");

			var email = DeserializeEmailFile(files.First());
			return email;
		}

		private static FakeEmailMessage DeserializeEmailFile(string file)
		{
			return JsonConvert.DeserializeObject<FakeEmailMessage>(File.ReadAllText(file));
		}

		private List<string> WaitForEmailFiles(int waitFor, Func<List<string>, bool> filesAction = null)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var files = new List<string>();

			if (filesAction == null)
				filesAction = list => list.Any();

			while (!filesAction(files) && stopwatch.ElapsedMilliseconds < waitFor * 1000)
			{
				files = GetEmailFiles().ToList();
				Thread.Sleep(500);
			}


			return files;
		}

		private List<FakeEmailMessage> WaitForEmailsWithCondition(int waitFor, Func<List<FakeEmailMessage>, bool> filesAction, Func<FakeEmailMessage, bool> condition = null)
		{
			if (condition == null)
				condition = x => true;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var emails = new List<FakeEmailMessage>();

			while (!filesAction(emails) && stopwatch.ElapsedMilliseconds < waitFor * 1000)
			{
				emails = GetEmailFiles().Select(DeserializeEmailFile).Where(condition).ToList();
				Thread.Sleep(500);
			}

			return emails;
		}

		public void DeleteEmails()
		{
			GetEmailFiles().ToList().ForEach(File.Delete);
		}

		private IEnumerable<string> GetEmailFiles()
		{
			return Directory.GetFiles(IntergrationHelpers.AssemblyDirectory).Where(x =>
			{
				var filename = Path.GetFileName(x);
				if (string.IsNullOrEmpty(filename))
					return false;

				return filename.StartsWith("email") &&
					   filename.EndsWith(".persist");
			});
		}

		public void AssertEmailNotSent(int waitFor = 30)
		{
			var files = WaitForEmailFiles(waitFor);

			if (files.Any())
			{
				Assert.Fail("Emails were found");
			}

		}

		public void AssertEmailsSentBy(string droneId, int numberOfEmails, int waitFor = 30)
		{
			var files = WaitForEmailsWithCondition(waitFor, list => list.Count == numberOfEmails, message => message.DroneId == droneId);

			if (files.Count != numberOfEmails)
			{
				Assert.Fail("The expected number of emails {0} was not sent, found {1}", numberOfEmails, files.Count);
			}
		}

		public void AssertEmailsSentWithInterval(IList<Recipient> recipients, int interval, int waitFor = 30)
		{
			var emails = WaitForEmailsWithCondition(waitFor, messages => recipients.Join(messages, x => x.Email, y => y.To.First().Address, (x, y) => x).Count() == recipients.Count());

			var deliveryTimes = emails
				.Select(x => x.DeliveryDate)
				.OrderBy(x => x.ToUniversalTime());

			deliveryTimes.AssertTimeDifferenceInRange(interval, 2);
		}

		public void AssertEmailsSentWithInterval(List<string> recipients, int interval, int waitFor = 30)
		{
			AssertEmailsSentWithInterval(recipients.Select(x => new Recipient { Email = x }).ToList(), interval);
		}
	}
}
