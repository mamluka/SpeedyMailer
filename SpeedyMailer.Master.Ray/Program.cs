using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CsvHelper;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Utilities.Extentions;

namespace SpeedyMailer.Master.Ray
{
	class Program
	{
		public class RayCommandOptions : CommandLineOptionsBase
		{
			[Option("p", "process-csv", HelpText = "The base url of the service to register the drone with")]
			public string CsvFile { get; set; }
			
			[Option("c", "create-list", HelpText = "The base url of the service to register the drone with")]
			public string InputF‎Ile { get; set; }

			[Option("T", "top", HelpText = "The base url of the service to register the drone with")]
			public bool ListTopDomains { get; set; }

			[Option("E", "estimate", HelpText = "The base url of the service to register the drone with")]
			public string EstimationParameters { get; set; }
		}

		static void Main(string[] args)
		{
			var rayCommandOptions = new RayCommandOptions();
			if (CommandLineParser.Default.ParseArguments(args, rayCommandOptions))
			{
				if (!string.IsNullOrEmpty(rayCommandOptions.CsvFile))
				{
					var csvSource = File.OpenRead(rayCommandOptions.CsvFile);
					var csvReader = new CsvReader(new StreamReader(csvSource));
					var rows = csvReader.GetRecords<ContactsListCsvRow>().ToList();

					WriteToConsole("There are {0} contacts", rows.Count);

					const int size = 3000;
					var chunks = rows.Clump(size).ToList();

					WriteToConsole("When devided to {0} created {1} chunks", size, chunks.Count);


					if (rayCommandOptions.ListTopDomains)
						TopDomains(rows);

					if (!string.IsNullOrEmpty(rayCommandOptions.EstimationParameters))
						CalculateSendingTime(rows, rayCommandOptions.EstimationParameters);
				}

				if (!string.IsNullOrEmpty(rayCommandOptions.InputF‎Ile))
				{
					using (var fr = new StreamReader(rayCommandOptions.InputF‎Ile))
					{
						var data = fr.ReadToEnd();
						var emails = data.Split(Convert.ToChar(Environment.NewLine));
						var csvRows = emails.Select(x => new ContactsListCsvRow
							{
								Email = x,
								City = "foo",
								Country = "foo",
								Firstname = "foo",
								Ip = "foo",
								Lastname = "foo",
								Phone = "foo",
								State = "foo",
								Zip = "foo",
							});

						
					}
				}

			}
		}

		private static void CalculateSendingTime(IList<ContactsListCsvRow> rows, string parameters)
		{
			var domains = rows
				.Select(x => Regex.Match(x.Email, "@(.+?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Groups[1].Value)
				.GroupBy(x => x.ToLower())
				.Select(x => new { x.Key, Count = x.Count() })
				.OrderByDescending(x => x.Count)
				.Where(x => x.Count > rows.Count() * 0.1)
				.ToList();

			var strings = parameters.Split(',');
			var numberOfDrones = int.Parse(strings[0]);
			var interval = int.Parse(strings[1]);

			WriteToConsole("We are considering {0} groups: {1}", domains.Count, string.Join(",", domains.Select(x => x.Key)));
			WriteSaperator();

			var speed = TimeSpan.FromSeconds(domains.First().Count * interval / numberOfDrones);

			WriteToConsole("The sending will take minimum of {0} hours or {1} days", speed.TotalHours, speed.TotalDays);
		}

		private static void TopDomains(IEnumerable<ContactsListCsvRow> rows)
		{
			var domains = rows
				.Select(x => Regex.Match(x.Email, "@(.+?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Groups[1].Value)
				.GroupBy(x => x.ToLower())
				.Select(x => new { x.Key, Count = x.Count() })
				.OrderByDescending(x => x.Count)
				.ToList();

			WriteToConsole("There are {0} groups", domains.Count);
			WriteToConsole("The top 10 domains are:");
			WriteSaperator();

			domains.Where(x => x.Count > 10).ToList().ForEach(x => WriteToConsole("Domain: {0} has: {1}", x.Key, x.Count));
			WriteSaperator();
		}

		private static void WriteSaperator()
		{
			Console.WriteLine("==========================================================================");
		}

		private static void WriteToConsole(string message, params object[] objects)
		{
			Console.WriteLine(message, objects);
		}
	}
}
