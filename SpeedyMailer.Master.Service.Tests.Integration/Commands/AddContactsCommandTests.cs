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

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class AddContactsCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenAListOfContactsAndAList_ShouldAddThemToTheStore()
        {

            const string listName = "MyList";
            var listId = UiActions.ExecuteCommand<CreateListCommand,string>(x =>
                                                                         {
                                                                             x.Name = listName;
                                                                         });
            var contacts = Fixture.CreateMany<Contact>(10).ToList();

            UiActions.ExecuteCommand<AddContactsCommand,long>(x=>
                                                      {
                                                          x.Contacts = contacts;
                                                          x.ListId = listId;
                                                      });

            var result = Store.Query<Contact>(x => x.MemberOf.Any(list=> list == listId));

            var resultNames = result.Select(x => x.Name).ToList();
            var names = contacts.Select(x => x.Name).ToList();

            resultNames.Should().BeEquivalentTo(names);
        }

        [Test]
        public void Execute_WhenWeHaveDuplicates_ShouldRemovetheDuplicates()
        {
            const string listName = "MyList";
            var listId = UiActions.ExecuteCommand<CreateListCommand, string>(x =>
            {
                x.Name = listName;
            });

            var contacts = Fixture.CreateMany<Contact>(10).ToList();
            var theDuplicate = contacts[9];

            contacts.Add(theDuplicate);

            UiActions.ExecuteCommand<AddContactsCommand, long>(x =>
            {
                x.Contacts = contacts;
                x.ListId = listId;

            });

            var result = Store.Query<Contact>(x => x.MemberOf.Any(list => list == listId));

            result.Should().HaveCount(10);
            result.Count(x => x.Name == theDuplicate.Name).Should().Be(1);

        }

		[Test]
		public void Execute_WhenImportingContacts_ShouldApplyIntervalLogicToTheBeforeSaving()
		{
			var listId = UiActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

			var contacts = CreateRowWithDomain("gmail.com", 20)
				.Union(CreateRowWithDomain("hotmail.com", 70))
				.Union(CreateRowWithDomain("aol.com", 50))
				.Union(CreateRowWithDomain("random.com", 200))
				.ToList();

			AddIntervalRules("gmail.com", "gmail", 20);
			AddIntervalRules("hotmail.com", "hotmail", 20);
			AddIntervalRules("aol.com", "aol", 20);

			UiActions.ExecuteCommand<AddContactsCommand, long>(x =>
			{
				x.Contacts = contacts;
				x.ListId = listId;

			});

			var result = Store.Query<Contact>();

			result.Should().HaveCount(20 + 70 + 50 + 200);

			result.Where(x => x.DomainGroup == "gmail").Should().HaveCount(20);
			result.Where(x => x.DomainGroup == "hotmail").Should().HaveCount(70);
			result.Where(x => x.DomainGroup == "aol").Should().HaveCount(50);
			result.Where(x => x.DomainGroup == "$default$").Should().HaveCount(200);

		}

		private IList<Contact> CreateRowWithDomain(string domain, int count)
		{
			var rows = Fixture.CreateMany<Contact>(count).ToList();
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
    }
}
