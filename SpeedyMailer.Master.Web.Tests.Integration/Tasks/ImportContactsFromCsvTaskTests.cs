using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Master.Web.Core.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.Tests.Integration.Tasks
{
	[TestFixture]
	public class ImportContactsFromCsvTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenAPerfectCSVListIsGiven_ShouldParseItAndWriteToDataBase()
		{
			var listId = UIActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var filename = GenerateFileName("sample", "csv");
			CreateContactsCSV(filename);

			var task = new ImportContactsFromCsvTask
						{
							ListId = listId,
							File = filename
						};

			UIActions.ExecuteTask(task);

			var result = Load<ImportContactsFromCsvTask>(task.Id);

			result.TaskResults.NumberOfContacts.Should().Be(10);
			result.TaskResults.Filename.Should().Be(filename);
		}


		[Test]
		public void Execute_WhenListContainsDuplicates_ShouldIgnoreThem()
		{
			var listId = UIActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var filename = GenerateFileName("sample", "csv");
			CreateContactsCSVWithDuplicate(filename);

			var task = new ImportContactsFromCsvTask
			{
				ListId = listId,
				File = filename
			};

			UIActions.ExecuteTask(task);

			var result = Load<ImportContactsFromCsvTask>(task.Id);

			result.TaskResults.NumberOfContacts.Should().Be(10);
			result.TaskResults.Filename.Should().Be(filename);
		}

		private string GenerateFileName(string seed, string extention)
		{
			return string.Format("{0}-{1}.{2}", seed, Guid.NewGuid(), extention);
		}

		private void CreateContactsCSVWithDuplicate(string filename)
		{
			var list = Fixture.CreateMany<ContactFromCSVRow>(10).ToList();
			list.Add(list.Last());

			CreateCSVFile(filename, list);
		}

		public void CreateCSVFile<T>(string filename, IEnumerable<T> list) where T : class
		{
			using (var textWriter = new StreamWriter(filename))
			{
				var csvWriter = new CsvWriter(textWriter);
				csvWriter.WriteRecords(list);
			}
		}

		private void CreateContactsCSV(string filename)
		{
			var list = Fixture.CreateMany<ContactFromCSVRow>(10);
			CreateCSVFile(filename, list);
		}

	}
}
