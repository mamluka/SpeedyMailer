using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using CommandLine;
using CsvHelper;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;

namespace SpeedyMailer.Master.Ray
{
	class Program
	{
		public class RayCommandOptions : CommandLineOptionsBase
		{
			[Option("p", "process-csv")]
			public string CsvFile { get; set; }

			[Option("o", "output-file")]
			public string OutputFile { get; set; }

			[Option("c", "create-list")]
			public string InputF‎Ile { get; set; }

			[Option("T", "top")]
			public bool ListTopDomains { get; set; }

			[Option("M", "max-count")]
			public int MaximalCountOfContacts { get; set; }

			[Option("t", "thread-numbers")]
			public int NumberOfThreads { get; set; }

			[Option("D", "check-dns")]
			public string CheckDns { get; set; }

			[Option("X", "extract-domains")]
			public bool ExtractDomains { get; set; }

			[Option("E", "estimate")]
			public string EstimationParameters { get; set; }
		}

		static void Main(string[] args)
		{
			var rayCommandOptions = new RayCommandOptions();
			if (CommandLineParser.Default.ParseArguments(args, rayCommandOptions))
			{
				if (!string.IsNullOrEmpty(rayCommandOptions.CsvFile))
				{
					var st = new Stopwatch();
					st.Start();
					var csvSource = File.OpenRead(rayCommandOptions.CsvFile);
					var csvReader = new CsvReader(new StreamReader(csvSource));
					var rows = csvReader.GetRecords<OneRawContactsListCsvRow>().ToList();
					st.Stop();

					WriteToConsole("There are {0} contacts, reading them took {1} seconds", rows.Count, st.ElapsedMilliseconds / 1000);

					st.Reset();
					st.Start();
					rows = rows.AsParallel().Distinct().ToList();
					rows.ForEach(x => x.Email = x.Email.ToLower());
					st.Stop();

					WriteToConsole("Doing distinct took {0} seconds", st.ElapsedMilliseconds / 1000);
					WriteToConsole("We now have {0} contacts", rows.Count);

					if (rayCommandOptions.ListTopDomains)
						TopDomains(rows);

					if (!string.IsNullOrEmpty(rayCommandOptions.EstimationParameters))
						CalculateSendingTime(rows, rayCommandOptions.EstimationParameters);

					if (rayCommandOptions.MaximalCountOfContacts > 0)
					{
						OutputSmallDomains(rows, rayCommandOptions);
					}

					if (rayCommandOptions.ExtractDomains)
					{
						var domains = GroupByDomain(rows).Select(x => x.Key);
						File.WriteAllLines(rayCommandOptions.OutputFile, domains);
					}
				}

				if (rayCommandOptions.CheckDns.HasValue())
				{
					var domains = File.ReadAllLines(rayCommandOptions.CheckDns);
					CheckDns(domains, rayCommandOptions);
				}
			}
			else
			{
				WriteToConsole("Parameter problem");
			}
		}

		private static void CheckDns(IEnumerable<string> domains, RayCommandOptions rayCommandOptions)
		{
			var st = new Stopwatch();
			st.Start();
			var version = Guid.NewGuid().ToString().Substring(0, 6);

			var error = new List<string>();

			var cleanDomains = domains
				.AsParallel()
				.Where(domain =>
				{
					var client = new DnsClient(IPAddress.Parse("8.8.8.8"), 10000);

					var mxRecords = client.Resolve(domain, RecordType.Mx);
					if (mxRecords != null && (mxRecords.ReturnCode == ReturnCode.NoError || mxRecords.AnswerRecords.OfType<MxRecord>().Any()))
						return true;

					var aRecord = client.Resolve(domain, RecordType.A);
					if (aRecord == null || aRecord.ReturnCode != ReturnCode.NoError || !aRecord.AnswerRecords.Any())
					{
						if (aRecord == null)
						{
							var message = "this domain produce null: " + domain;
							WriteToConsole(message);
							error.Add(message);
						}

						if (aRecord != null)
						{
							var message = aRecord.ReturnCode + " dns error for: " + domain;
							WriteToConsole(message);

							error.Add(message);
						}


						return false;
					}

					return CanConnect(aRecord.AnswerRecords.OfType<ARecord>().First().Address, domain);
				}).ToList();

			st.Stop();

			WriteToConsole("Total shabank took: " + st.ElapsedMilliseconds);

			File.WriteAllLines(rayCommandOptions.OutputFile, cleanDomains);
			File.WriteAllLines(rayCommandOptions.OutputFile + ".bad.txt", domains.Except(cleanDomains));
			File.WriteAllLines(rayCommandOptions.OutputFile + ".error.log." + version + ".txt", error.OrderBy(x => x).ToList());
		}

		private static bool CanConnect(IPAddress ip, string domain)
		{
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var result = socket.BeginConnect(ip, 25, null, null);

			var st = new Stopwatch();
			st.Start();
			bool success = result.AsyncWaitHandle.WaitOne(5000, true);
			st.Stop();

			WriteToConsole(domain + " connecton took: " + st.ElapsedMilliseconds);

			if (!success)
			{
				socket.Close();
				return false;
			}

			return true;
		}

		private static void OutputSmallDomains(List<OneRawContactsListCsvRow> rows, RayCommandOptions rayCommandOptions)
		{
			var st = new Stopwatch();

			st.Start();
			var removeDomains = GroupByDomain(rows)
				.Where(x => x.Count() > rayCommandOptions.MaximalCountOfContacts)
				.Select(x => x.Key)
				.Where(x => x.HasValue())
				.ToList();

			st.Stop();

			WriteToConsole("Group by took {0} seconds", st.ElapsedMilliseconds / (long)1000);

			WriteToConsole("There are {0} domains to remove, they are {1}", removeDomains.Count, removeDomains.Commafy());

			st.Reset();
			st.Start();
			var newRows = RemoveRowsByDomains(rows, removeDomains);
			st.Stop();

			WriteToConsole("Removing domains took {0} ms", st.ElapsedMilliseconds);

			st.Reset();
			st.Start();
			WriteCsv(rayCommandOptions, newRows);
			st.Stop();

			WriteToConsole("Writing the CSV took {0} ms", st.ElapsedMilliseconds);
		}

		private static List<OneRawContactsListCsvRow> RemoveRowsByDomains(List<OneRawContactsListCsvRow> rows, List<string> removeDomains)
		{
			removeDomains = removeDomains.Select(x => "@" + x).ToList();

			return rows
				.AsParallel()
				.Where(x => !removeDomains.Any(r => x.Email.EndsWith(r)))
				.ToList();

			//			return rows
			//				.Where(x => !Regex.Match(x.Email, string.Join("|", removeDomains)).Success)
			//				.ToList();
		}

		private static void WriteCsv(RayCommandOptions rayCommandOptions, IEnumerable<OneRawContactsListCsvRow> newRows)
		{
			using (var textWriter = new StreamWriter(rayCommandOptions.OutputFile))
			{
				var csvWriter = new CsvWriter(textWriter);
				csvWriter.WriteRecords(newRows);
			}
		}

		private static void CalculateSendingTime(IList<OneRawContactsListCsvRow> rows, string parameters)
		{
			var domains = GroupByDomain(rows)
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

		private static IEnumerable<IGrouping<string, string>> GroupByDomain(IEnumerable<OneRawContactsListCsvRow> rows)
		{
			return rows
				.AsParallel()
				.Select(x => Regex.Match(x.Email, "@(.+?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Groups[1].Value)
				.GroupBy(x => x.ToLower());
		}

		private static void TopDomains(IEnumerable<OneRawContactsListCsvRow> rows)
		{
			var domains = rows
				.Select(GetDomain)
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

		private static string GetDomain(OneRawContactsListCsvRow x)
		{
			return Regex.Match(x.Email, "@(.+?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Groups[1].Value;
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
