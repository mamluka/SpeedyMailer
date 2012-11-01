using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Master.Service.Tests.Integration.Tasks
{
	[TestFixture]
	public class ImportContactsFromCsvTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenAPerfectCSVListIsGiven_ShouldParseItAndWriteToDataBase()
		{
			var listId = UiActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var filename = CsvTestingExtentions.GenerateFileName("sample");
			Fixture.CreateMany<ContactsListCsvRow>(10).ToCsvFile(filename);

			var task = new ImportContactsFromCsvTask
						{
							ListId = listId,
							File = filename
						};

			UiActions.ExecuteTask(task);

			var result = Store.Load<ImportContactsFromCsvTask>(task.Id);

			result.TaskResults.NumberOfContacts.Should().Be(10);
			result.TaskResults.Filename.Should().Be(filename);
		}

		[Test]
		public void Execute_WhenImportingContacts_ShouldApplyIntervalLogicToTheBeforeSaving()
		{
			var listId = UiActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var filename = CsvTestingExtentions.GenerateFileName("sample");
			var rows = CreateRowWithDomain("gmail.com", 20)
				.Union(CreateRowWithDomain("hotmail.com", 70))
				.Union(CreateRowWithDomain("aol.com", 50))
				.Union(CreateRowWithDomain("random.com", 200))
				.ToList();

			rows.ToCsvFile(filename);

			AddIntervalRules("gmail.com","gmail",20);
			AddIntervalRules("hotmail.com","hotmail",20);
			AddIntervalRules("aol.com","aol",20);


			var task = new ImportContactsFromCsvTask
						{
							ListId = listId,
							File = filename
						};

			UiActions.ExecuteTask(task);

			Tasks.WaitForTaskToComplete(task.Id);

			var result = Store.Query<Contact>();

			result.Should().HaveCount(20 + 70 + 50 + 200);

			result.Where(x => x.DomainGroup == "gmail").Should().HaveCount(20);
			result.Where(x => x.DomainGroup == "hotmail").Should().HaveCount(70);
			result.Where(x => x.DomainGroup == "aol").Should().HaveCount(50);
			result.Where(x => x.DomainGroup == "$default$").Should().HaveCount(200);

		}

		private IList<ContactsListCsvRow> CreateRowWithDomain(string domain, int count)
		{
			var rows = Fixture.CreateMany<ContactsListCsvRow>(count).ToList();
			rows.ForEach(x => x.Email = Guid.NewGuid() + "@" + domain);

			return rows;
		}

		private void AddIntervalRules(string address, string group, int interval)
		{
			ServiceActions.ExecuteCommand<AddIntervalRulesCommand>(x => x.Rules = new[]
				                                                                      {
					                                                                      new IntervalRule
						                                                                      {
																								  Conditons = new List<string> { address},
																								  Group = group,
																								  Interval = interval
						                                                                      }
				                                                                      });
		}


		[Test]
		public void Execute_WhenListContainsDuplicates_ShouldIgnoreThem()
		{
			var listId = UiActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var filename = CsvTestingExtentions.GenerateFileName("sample");

			var list = Fixture.CreateMany<ContactsListCsvRow>(10).ToList();
			list.Add(list.Last());

			list.ToCsvFile(filename);

			var task = new ImportContactsFromCsvTask
			{
				ListId = listId,
				File = filename
			};

			UiActions.ExecuteTask(task);

			var result = Store.Load<ImportContactsFromCsvTask>(task.Id);

			result.TaskResults.NumberOfContacts.Should().Be(10);
			result.TaskResults.Filename.Should().Be(filename);
		}
	}
}
