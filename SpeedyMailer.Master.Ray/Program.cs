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

			}
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
