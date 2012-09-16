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
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Master.Web.Tests.Integration.Tasks
{
	[TestFixture]
	public class ImportContactsFromCsvTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenAPerfectCSVListIsGiven_ShouldParseItAndWriteToDataBase()
		{
			var listId = UIActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var filename = CsvTestingExtentions.GenerateFileName("sample");
			Fixture.CreateMany<ContactsListCsvRow>(10).ToCsvFile(filename);

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

			var filename = CsvTestingExtentions.GenerateFileName("sample");

			var list = Fixture.CreateMany<ContactsListCsvRow>(10).ToList();
			list.Add(list.Last());

			list.ToCsvFile(filename);

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
	}
}
