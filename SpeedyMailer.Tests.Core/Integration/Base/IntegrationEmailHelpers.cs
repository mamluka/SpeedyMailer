using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
		public void AssertEmailSent(Expression<Func<FakeEmailMessage, bool>> func, int waitFor = 5)
		{
			var email = GetEmailFromDisk(waitFor);
			email.Should().Match(func);
		}

		public void AssertEmailSent(Action<FakeEmailMessage> action, int waitFor = 5)
		{
			var email = GetEmailFromDisk(waitFor);
			action(email);
		}

		public void AssertEmailsSentTo(IEnumerable<string> recipients, int waitFor = 5)
		{
			var emails = WaitForEmailsThatWereSentBy(recipients, waitFor);

			if (emails.Count != recipients.Count())
				Assert.Fail("Emails were not sent to all recipients expected {0} but {1} were sent", recipients.Count(), emails.Count);
		}

		private List<FakeEmailMessage> WaitForEmailsThatWereSentBy(IEnumerable<string> recipients, int waitFor)
		{
			return WaitForEmailsWithCondition(waitFor,
											  messages => recipients.Join(messages, x => x, y => y.To.First().Address, (x, y) => x)
															  .Count() == recipients.Count(), x => recipients.Any(m => x.To.First().Address == m));
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
			while (true)
			{
				try
				{
					var emailFile = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
					using (var streamReader = new StreamReader(emailFile))
					{
						return JsonConvert.DeserializeObject<FakeEmailMessage>(streamReader.ReadToEnd());
					}
				}
				catch
				{
					Thread.Sleep(200);
				}
			}
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

		public void AssertEmailNotSent(int waitFor = 5)
		{
			var files = WaitForEmailFiles(waitFor);

			if (files.Any())
			{
				Assert.Fail("Emails were found");
			}

		}

		public void AssertEmailNotSent(IList<Recipient> recipients, int waitFor = 5)
		{
			AssertEmailNotSent(recipients.Select(x => x.Email).ToList(), waitFor);
		}

		public void AssertEmailNotSent(IList<string> recipients, int waitFor = 5)
		{
			var emails = WaitForEmailsThatWereSentBy(recipients, waitFor);

			if (emails.Any())
			{
				Assert.Fail("Emails were found");
			}
		}

		public void AssertEmailSent(int count, int waitFor = 5)
		{
			var files = WaitForEmailFiles(waitFor, x => x.Count == count);

			files.Should().HaveCount(count);
		}

		public void AssertEmailsSentBy(string droneId, int numberOfEmails, int waitFor = 5)
		{
			var files = WaitForEmailsWithCondition(waitFor, list => list.Count == numberOfEmails, message => message.DroneId == droneId);

			if (files.Count != numberOfEmails)
			{
				Assert.Fail("The expected number of emails {0} was not sent, found {1}", numberOfEmails, files.Count);
			}
		}

		public void AssertEmailsSentWithInterval(IList<Recipient> recipients, int interval, int waitFor = 5)
		{
			var emails = WaitForEmailsThatWereSentBy(recipients.Select(x => x.Email), waitFor);

			emails.Should().HaveCount(recipients.Count, "Not all emails were sent", emails.Count, recipients.Count);

			var deliveryTimes = OrderByDeliveryTimes(emails);
			deliveryTimes.AssertTimeDifferenceInRange(interval, 1);
		}

		private static IOrderedEnumerable<DateTime> OrderByDeliveryTimes(List<FakeEmailMessage> emails)
		{
			return emails
				.Select(x => x.DeliveryDate)
				.OrderBy(x => x.ToUniversalTime());
		}

		public void AssertEmailsSentWithInterval(List<string> recipients, int interval, int waitFor = 5)
		{
			AssertEmailsSentWithInterval(recipients.Select(x => new Recipient { Email = x }).ToList(), interval, waitFor);
		}

		public void WaitForEmailsToBeSent(List<string> recipients, int waitFor = 5)
		{
			WaitForEmailsWithCondition(waitFor,
									   messages => recipients.Join(messages, x => x, y => y.To.First().Address, (x, y) => x)
									   .Count() == recipients.Count());

		}

		public void AssertEmailsWereSendAtTheSameTime(IEnumerable<Recipient> recipients, int waitFor = 5)
		{
			var emails = WaitForEmailsThatWereSentBy(recipients.Select(x => x.Email), waitFor);

			var deliveryTimes = OrderByDeliveryTimes(emails);
			deliveryTimes.AssertTimesAreTheSameInRange(200);
		}

		public void AssertEmailsSentInOrder(IList<string> recipients, int waitFor = 5)
		{
			var emails = WaitForEmailsWithCondition(waitFor,
									   messages => messages.OrderBy(x => x.SendTime).Select(x => x.To[0].Address).SequenceEqual(recipients));

			emails.OrderBy(x => x.SendTime).Select(x => x.To[0].Address).Should().ContainInOrder(recipients);
		}
	}
}
