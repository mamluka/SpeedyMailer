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
			[Option("p", "process-csv", HelpText = "The base url of the service to register the drone with")]
			public string CsvFile { get; set; }

			[Option("o", "output-file", HelpText = "The base url of the service to register the drone with")]
			public string OutputFile { get; set; }

			[Option("c", "create-list", HelpText = "The base url of the service to register the drone with")]
			public string InputF‎Ile { get; set; }

			[Option("T", "top", HelpText = "The base url of the service to register the drone with")]
			public bool ListTopDomains { get; set; }

			[Option("M", "max-count", HelpText = "The base url of the service to register the drone with")]
			public int MaximalCountOfContacts { get; set; }

			[Option("D", "check-dns", HelpText = "The base url of the service to register the drone with")]
			public bool CheckDns { get; set; }

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
					st.Stop();

					WriteToConsole("Doing distinct took {0} seconds", st.ElapsedMilliseconds / 1000);

					if (rayCommandOptions.ListTopDomains)
						TopDomains(rows);

					if (!string.IsNullOrEmpty(rayCommandOptions.EstimationParameters))
						CalculateSendingTime(rows, rayCommandOptions.EstimationParameters);

					if (rayCommandOptions.MaximalCountOfContacts > 0)
					{
						OutputSmallDomains(rows, rayCommandOptions);
					}

					if (rayCommandOptions.CheckDns)
					{
						var domains = GroupByDomain(rows).ToList();
						var counter = 0;
						var total = new Stopwatch();
						total.Start();

						var badDomains = domains.AsParallel().Where(x =>
							{
								var s = new Stopwatch();
								counter++;
								try
								{

									s.Start();
									var hostInfo = Dns.GetHostEntry(x.Key);
									s.Stop();
									Console.WriteLine(counter + " We are resolving domain:" + x.Key + " it took: " + s.ElapsedMilliseconds);

									var host = x.Key;

									var mxRecord = DnsClient.Default.Resolve(x.Key, RecordType.Mx);


									if (!((mxRecord == null) || ((mxRecord.ReturnCode != ReturnCode.NoError) && (mxRecord.ReturnCode != ReturnCode.NxDomain))))
									{
										var mxHost = mxRecord.AnswerRecords.OfType<MxRecord>().Select(record => record.ExchangeDomainName).LastOrDefault();

										if (mxHost.HasValue())
										{
											host = mxHost;
											Console.WriteLine("MX records found for: " + x.Key + " they are: " + host);
										}
									}

									using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
									{
										try
										{
											socket.Connect(host, 25);
										}
										catch (SocketException ex)
										{
											Console.WriteLine("Can't connect to {0} : {1} - {2}", host, ex.SocketErrorCode, ex.Message);
											return true;
										}
									}

									return !hostInfo.AddressList.Any();
								}
								catch (Exception)
								{
									s.Stop();
									Console.WriteLine(counter + " We are resolving domain:" + x.Key + " resolve failed and it took: " + s.ElapsedMilliseconds);
									return true;
								}
							}).Select(x => x.Key)
							.ToList();

						var afterDns = RemoveRowsByDomains(rows, badDomains);

						WriteCsv(rayCommandOptions, afterDns);

						total.Stop();

						Console.WriteLine("Total resolve took:" + total.ElapsedMilliseconds / (1000 * 60) + " m");
						Console.ReadKey();
					}
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

			WriteToConsole("There are {0} domains to remove", removeDomains.Count);

			st.Reset();
			st.Start();
			var newRows = RemoveRowsByDomains(rows, removeDomains);
			st.Stop();

			WriteToConsole("Removing domains took {0} seconds", st.ElapsedMilliseconds / (long)1000);

			st.Reset();
			st.Start();
			WriteCsv(rayCommandOptions, newRows);
			st.Stop();

			WriteToConsole("Writing the CSV took {0} seconds", st.ElapsedMilliseconds / 1000);
		}

		private static List<OneRawContactsListCsvRow> RemoveRowsByDomains(List<OneRawContactsListCsvRow> rows, List<string> removeDomains)
		{
			removeDomains = removeDomains.Select(x => "@" + x + "$").ToList();
			return rows
				.AsParallel()
				.Where(x => !removeDomains.Any(r => Regex.Match(x.Email, r, RegexOptions.IgnoreCase | RegexOptions.Compiled).Success))
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
