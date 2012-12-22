using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;
using Ploeh.AutoFixture;

namespace SpeedyMailer.Master.Service.Tests.Integration.Tasks
{
	[TestFixture]
	public class CreateCreativeFragmentsTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenACreativeIsGiven_ShouldStoreTheFragmentsWithRecipientsDevidedBetweenTheFragments()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 1000);

			var listId = UiActions.CreateListWithRandomContacts("MyList", 1500);

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
													  {
														  x.HtmlBody = "Body";
														  x.TextBody = "text body";
														  x.Subject = "Subject";
														  x.UnsubscribeTemplateId = templateId;
														  x.Lists = new List<string> { listId };
													  });

			var task = new CreateCreativeFragmentsTask
						{
							CreativeId = creativeId,
						};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>(x => x.CreativeId == creativeId);

			result.Should().HaveCount(2);
			result.First().Recipients.Should().HaveCount(1000);
			result.Second().Recipients.Should().HaveCount(500);
		}

		[Test]
		public void Execute_WhenACreativeIsGiven_ShouldStoreAllRelevantDataForTheRecipients()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 1000);

			var contact = Fixture.CreateAnonymous<Contact>();

			var listId = UiActions.CreateListWithContacts("MyList", new[] { contact });

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
													  {
														  x.HtmlBody = "Body";
														  x.Subject = "Subject";
														  x.UnsubscribeTemplateId = templateId;
														  x.Lists = new List<string> { listId };
													  });

			var task = new CreateCreativeFragmentsTask
						{
							CreativeId = creativeId,
						};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>(x => x.CreativeId == creativeId);

			result.First().Recipients.Should().HaveCount(1)
				.And.OnlyContain(x =>
								 x.ContactId == contact.Id &&
								 x.Name == contact.Name &&
								 x.Email == contact.Email
				);
		}

		[Test]
		public void Execute_WhenACreativeIsGiven_ShouldStoreAllRelevantDataForTheCreative()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 1000);

			var contact = Fixture.CreateAnonymous<Contact>();

			var listId = UiActions.CreateListWithContacts("MyList", new[] { contact });

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
													  {
														  x.HtmlBody = "Body";
														  x.TextBody = "Body text";
														  x.Subject = "Subject";
														  x.UnsubscribeTemplateId = templateId;
														  x.FromAddressDomainPrefix = "sales";
														  x.FromName = "david";
														  x.DealUrl = "http://deal.com";
														  x.Lists = new List<string> { listId };
													  });

			var task = new CreateCreativeFragmentsTask
						{
							CreativeId = creativeId,
						};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>(x => x.CreativeId == creativeId).First();

			result.HtmlBody.Should().Be("Body");
			result.TextBody.Should().Be("Body text");
			result.Subject.Should().Be("Subject");
			result.FromName.Should().Be("david");
			result.FromAddressDomainPrefix.Should().Be("sales");
			result.UnsubscribeTemplate.Should().Be("Body");
			result.DealUrl.Should().Be("http://deal.com");
		}
		
		[Test]
		public void Execute_WhenGivenIntervalRules_ShouldSetTheCorrectItnervalsAndGroup()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 200);

			var rule = new IntervalRule
			{
				Conditons = new List<string> { "gmail.com", "hotmail.com" },
				Interval = 10,
				Group = "gmail"
			};

			ServiceActions.ExecuteCommand<AddIntervalRulesCommand>(x => x.Rules = new[] { rule });

			var contacts = AddRandomContacts(100);

			var topDomainContacts = new List<Contact>
			                   {
				                   CreateContactWithEmail("user1@gmail.com"),
				                   CreateContactWithEmail("user2@gmail.com"),
				                   CreateContactWithEmail("user3@hotmail.com"),
				                   CreateContactWithEmail("user4@hotmail.com"),
				                   CreateContactWithEmail("user5@hotmail.com"),
			                   };

			var listId = CraeteListFromContacts("my list", contacts.Union(topDomainContacts));

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.HtmlBody = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});



			var task = new CreateCreativeFragmentsTask
			{
				CreativeId = creativeId,
			};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>().First();

			result.Recipients.Should().Contain(x => topDomainContacts.Any(contact => contact.Email == x.Email) && x.Interval == 10 && x.Group == "gmail");
		}

		[Test]
		public void Execute_WhenGivenIntervalRulesAndContactsThatNotAllOfThemMatchAllTheInteravlRules_ShouldSetTheCorrectItnervalsAndGroup()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 200);

			var rules = new[]
				            {
					            new IntervalRule
						            {
							            Conditons = new List<string> {"gmail.com"},
							            Interval = 10,
							            Group = "gmail"
						            },
								new IntervalRule
						            {
							            Conditons = new List<string> {"aol.com"},
							            Interval = 10,
							            Group = "aol"
						            }
				            };

			ServiceActions.ExecuteCommand<AddIntervalRulesCommand>(x => x.Rules = rules);

			var contacts = AddRandomContacts(100);

			var topDomainContacts = new List<Contact>
			                   {
				                   CreateContactWithEmail("user1@gmail.com"),
				                   CreateContactWithEmail("user2@gmail.com"),
			                   };

			var listId = CraeteListFromContacts("my list", contacts.Union(topDomainContacts));

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.HtmlBody = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});



			var task = new CreateCreativeFragmentsTask
			{
				CreativeId = creativeId,
			};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>().First();

			result.Recipients.Should().Contain(x => topDomainContacts.Any(contact => contact.Email == x.Email) && x.Interval == 10 && x.Group == "gmail");
		}

		[Test]
		public void Execute_WhenThereAreNoIntervalRules_ShouldSetTheIntervalAndGroupToTheDefaultValue()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x =>
																	  {
																		  x.DefaultInterval = 1;
																		  x.RecipientsPerFragment = 200;
																	  });

			var contacts = AddRandomContacts(200);

			var listId = CraeteListFromContacts("my list", contacts);

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.HtmlBody = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});

			var task = new CreateCreativeFragmentsTask
			{
				CreativeId = creativeId,
			};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>().First();

			result.Recipients.Should().OnlyContain(x => x.Interval == 1);
			result.Recipients.Should().OnlyContain(x => x.Group == "$default$");
		}

		[Test]
		public void Execute_WhenGivenSeveralIntervalRules_ShouldDistributeTheDomainsEvenlyInsideTheFragment()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 920);

			var group1Contacts = CreateDomainContacts("hotmail.com", 217);
			var group2Contacts = CreateDomainContacts("gmail.com", 213);
			var group3Contacts = CreateDomainContacts("aol.com", 553);
			var group4Contacts = CreateDomainContacts("msn.com", 337);
			var randomContacts = CreateDomainContacts("random.com", 777);

			AddIntervalRules("hotmail.com", "hotmail", 30);
			AddIntervalRules("gmail.com", "gmail", 20);
			AddIntervalRules("aol.com", "aol", 10);
			AddIntervalRules("msn.com", "msn", 15);

			var listId = CraeteListFromContacts("my list", group1Contacts.Union(group2Contacts).Union(group3Contacts).Union(group4Contacts).Union(randomContacts).OrderBy(x => Guid.NewGuid()));

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.HtmlBody = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});

			var task = new CreateCreativeFragmentsTask
			{
				CreativeId = creativeId,
			};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>();

			var firstFragment = result[0];

			firstFragment.Recipients.Should().HaveCount(920);
			AssertDomainCountIn(firstFragment.Recipients, "hotmail.com", 96);
			AssertDomainCountIn(firstFragment.Recipients, "gmail.com", 93);
			AssertDomainCountIn(firstFragment.Recipients, "aol.com", 242);
			AssertDomainCountIn(firstFragment.Recipients, "msn.com", 148);
			AssertDomainCountIn(firstFragment.Recipients, "random.com", 341);

			var secondFragment = result[1];

			secondFragment.Recipients.Should().HaveCount(920);
			AssertDomainCountIn(secondFragment.Recipients, "hotmail.com", 95);
			AssertDomainCountIn(secondFragment.Recipients, "gmail.com", 94);
			AssertDomainCountIn(secondFragment.Recipients, "aol.com", 242);
			AssertDomainCountIn(secondFragment.Recipients, "msn.com", 148);
			AssertDomainCountIn(secondFragment.Recipients, "random.com", 341);

			var lastFragment = result.First(x => x.Recipients.Count == 257);

			AssertDomainCountIn(lastFragment.Recipients, "hotmail.com", 26);
			AssertDomainCountIn(lastFragment.Recipients, "gmail.com", 26);
			AssertDomainCountIn(lastFragment.Recipients, "aol.com", 69);
			AssertDomainCountIn(lastFragment.Recipients, "msn.com", 41);
			AssertDomainCountIn(lastFragment.Recipients, "random.com", 95);
		}

		[Test]
		public void Execute_WhenGivenSeveralIntervalRulesAndAnotherListAlreadyExists_ShouldDistributeTheDomainsEvenlyInsideTheFragment()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 920);

			var group1Contacts = CreateDomainContacts("hotmail.com", 217);
			var group2Contacts = CreateDomainContacts("gmail.com", 213);
			var group3Contacts = CreateDomainContacts("aol.com", 553);
			var group4Contacts = CreateDomainContacts("msn.com", 337);
			var randomContacts = CreateDomainContacts("random.com", 777);

			AddIntervalRules("hotmail.com", "hotmail", 30);
			AddIntervalRules("gmail.com", "gmail", 20);
			AddIntervalRules("aol.com", "aol", 10);
			AddIntervalRules("msn.com", "msn", 15);

			var listId = CraeteListFromContacts("my list", group1Contacts.Union(group2Contacts).Union(group3Contacts).Union(group4Contacts).Union(randomContacts).OrderBy(x => Guid.NewGuid()));

			CraeteListFromContacts("my seocnd list", CreateDomainContacts("antoherdomain.com", 1252));

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.HtmlBody = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});

			var task = new CreateCreativeFragmentsTask
			{
				CreativeId = creativeId,
			};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>();

			var firstFragment = result[0];

			firstFragment.Recipients.Should().HaveCount(920);
			AssertDomainCountIn(firstFragment.Recipients, "hotmail.com", 96);
			AssertDomainCountIn(firstFragment.Recipients, "gmail.com", 93);
			AssertDomainCountIn(firstFragment.Recipients, "aol.com", 242);
			AssertDomainCountIn(firstFragment.Recipients, "msn.com", 148);
			AssertDomainCountIn(firstFragment.Recipients, "random.com", 341);
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

		private void AssertDomainCountIn(IEnumerable<Recipient> recipients, string domain, int count)
		{
			recipients.Where(x => x.Email.Contains(domain)).Should().HaveCount(count);
		}

		private IList<Contact> CreateDomainContacts(string domain, int count)
		{
			return Enumerable.Range(0, count).Select(x => CreateContactWithEmail(Guid.NewGuid() + "@" + domain)).ToList();
		}

		private string CraeteListFromContacts(string listName, IEnumerable<Contact> contacts)
		{
			var listId = ServiceActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = listName);
			ServiceActions.ExecuteCommand<AddContactsCommand, long>(x =>
																  {
																	  x.ListId = listId;
																	  x.Contacts = contacts;
																  });

			return listId;
		}

		private IEnumerable<Contact> AddRandomContacts(int numberOfContacts)
		{
			return Fixture.Build<Contact>().Without(x => x.Id).CreateMany(numberOfContacts);
		}

		private Contact CreateContactWithEmail(string email)
		{
			return new Contact
					   {
						   Email = email,
						   Name = Guid.NewGuid().ToString(),
						   Country = Guid.NewGuid().ToString(),
					   };
		}

		private string CreateTemplate(string templateBody)
		{
			var templateId = UiActions.ExecuteCommand<CreateTemplateCommand, string>(x => { x.Body = templateBody; });
			return templateId;
		}
	}
}
