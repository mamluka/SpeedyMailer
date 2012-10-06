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

			var listId = UIActions.CreateListWithRandomContacts("MyList", 1500);

			var templateId = CreateTemplate("Body");

			var creativeId = UIActions.ExecuteCommand<AddCreativeCommand, string>(x =>
													  {
														  x.Body = "Body";
														  x.Subject = "Subject";
														  x.UnsubscribeTemplateId = templateId;
														  x.Lists = new List<string> { listId };
													  });

			var task = new CreateCreativeFragmentsTask
						{
							CreativeId = creativeId,
							RecipientsPerFragment = 1000
						};

			ServiceActions.ExecuteTask(task);

			var result = Query<CreativeFragment>(x => x.CreativeId == creativeId);

			result.Should().HaveCount(2);
			result.First().Recipients.Should().HaveCount(1000);
			result.Second().Recipients.Should().HaveCount(500);

			result.First().UnsubscribeTemplate.Should().Be("Body");
			result.Second().UnsubscribeTemplate.Should().Be("Body");
		}

		[Test]
		public void Execute_WhenACreativeIsGiven_ShouldReturnTheServiceEndpoints()
		{

			ServiceActions.EditSettings<ServiceSettings>(x => x.BaseUrl = DefaultBaseUrl);

			var listId = UIActions.CreateListWithRandomContacts("MyList", 700);

			var templateId = CreateTemplate("Body");

			var creativeId = UIActions.ExecuteCommand<AddCreativeCommand, string>(x =>
													  {
														  x.Body = "Body";
														  x.Subject = "Subject";
														  x.UnsubscribeTemplateId = templateId;
														  x.Lists = new List<string> { listId };
													  });

			var task = new CreateCreativeFragmentsTask
						{
							CreativeId = creativeId,
							RecipientsPerFragment = 1000
						};

			ServiceActions.ExecuteTask(task);

			var result = Query<CreativeFragment>().First();

			result.Service.BaseUrl.Should().Be(DefaultBaseUrl);
			result.Service.DealsEndpoint.Should().Be("deals");
			result.Service.UnsubscribeEndpoint.Should().Be("lists/unsubscribe");
		}

		[Test]
		public void Execute_WhenGivenIntervalRules_ShouldSetTheCorrectItnervals()
		{
			var contacts = new List<Contact>
			                   {
				                   CreateContactWithEmailDomain("gmail.com"),
				                   CreateContactWithEmailDomain("gmail.com"),
				                   CreateContactWithEmailDomain("hotmail.com"),
				                   CreateContactWithEmailDomain("hotmail.com"),
				                   CreateContactWithEmailDomain("hotmail.com"),
			                   };

			contacts.AddRange(AddRandomContacts(100));

			var listId = CraeteListFromContacts("my list", contacts);

			var templateId = CreateTemplate("Body");

			var creativeId = UIActions.ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.Body = "Body";
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = templateId;
				x.Lists = new List<string> { listId };
			});

			var rules = new IntervalRule
				            {
								Conditon = new List<string> { "gmail.com","hotmail.com"},
								Interval = 10
				            };


			var task = new CreateCreativeFragmentsTask
			{
				CreativeId = creativeId,
				RecipientsPerFragment = 200
			};

			ServiceActions.ExecuteTask(task);

			var result = Query<CreativeFragment>(x => x.CreativeId == creativeId);

			result.Should().HaveCount(2);
			result.First().Recipients.Should().HaveCount(1000);
			result.Second().Recipients.Should().HaveCount(500);

			result.First().UnsubscribeTemplate.Should().Be("Body");
			result.Second().UnsubscribeTemplate.Should().Be("Body");
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

		private Contact CreateContactWithEmailDomain(string domain)
		{
			return new Contact
					   {
						   Email = Guid.NewGuid() + "@" + domain,
						   Name = Guid.NewGuid().ToString(),
						   Country = Guid.NewGuid().ToString(),
					   };
		}

		private string CreateTemplate(string templateBody)
		{
			var templateId = UIActions.ExecuteCommand<CreateTemplateCommand, string>(x => { x.Body = templateBody; });
			return templateId;
		}
	}
}
