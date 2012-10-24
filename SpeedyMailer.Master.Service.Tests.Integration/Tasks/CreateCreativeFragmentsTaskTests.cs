using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;

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
														  x.Body = "Body";
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
														  x.Body = "Body";
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
														  x.Body = "Body";
														  x.Subject = "Subject";
														  x.UnsubscribeTemplateId = templateId;
														  x.FromAddressDomainPrefix = "sales";
														  x.FromName = "david";
														  x.Lists = new List<string> { listId };
													  });

			var task = new CreateCreativeFragmentsTask
						{
							CreativeId = creativeId,
						};

			ServiceActions.ExecuteTask(task);

			var result = Store.Query<CreativeFragment>(x => x.CreativeId == creativeId).First();

			result.Body.Should().Be("Body");
			result.Subject.Should().Be("Subject");
			result.FromName.Should().Be("david");
			result.FromAddressDomainPrefix.Should().Be("sales");
			result.UnsubscribeTemplate.Should().Be("Body");

		}

		[Test]
		public void Execute_WhenACreativeIsGiven_ShouldReturnTheServiceEndpoints()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 1000);
			ServiceActions.EditSettings<ServiceSettings>(x => x.BaseUrl = DefaultBaseUrl);

			var listId = UiActions.CreateListWithRandomContacts("MyList", 700);

			var templateId = CreateTemplate("Body");

			var creativeId = UiActions.ExecuteCommand<AddCreativeCommand, string>(x =>
													  {
														  x.Body = "Body";
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

			result.Service.BaseUrl.Should().Be(DefaultBaseUrl);
			result.Service.DealsEndpoint.Should().Be("deals");
			result.Service.UnsubscribeEndpoint.Should().Be("lists/unsubscribe");
		}

		[Test]
		public void Execute_WhenGivenIntervalRules_ShouldSetTheCorrectItnervalsAndGroup()
		{
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = 200);
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
				x.Body = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});

			var rule = new IntervalRule
							{
								Conditons = new List<string> { "gmail.com", "hotmail.com" },
								Interval = 10,
								Group = "gmail"
							};

			ServiceActions.ExecuteCommand<AddIntervalRulesCommand>(x => x.Rules = new[] { rule });

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
				x.Body = "Body";
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
			result.Recipients.Should().OnlyContain(x => x.Group == "default");
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
