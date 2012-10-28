using System;
using System.Collections.Generic;
using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using System.Linq;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;

namespace SpeedyMailer.Master.Service.Tasks
{
	public class CreateCreativeFragmentsTask : PersistentTask
	{
		public string CreativeId { get; set; }
	}

	public class CreateCreativeFragmentsTaskExecutor : PersistentTaskExecutor<CreateCreativeFragmentsTask>
	{
		private readonly IDocumentStore _documentStore;
		private readonly CreativeEndpointsSettings _creativeEndpointsSettings;
		private readonly ServiceSettings _serviceSettings;
		private readonly CreativeFragmentSettings _creativeFragmentSettings;

		public CreateCreativeFragmentsTaskExecutor(IDocumentStore documentStore, CreativeEndpointsSettings creativeEndpointsSettings, ServiceSettings serviceSettings, CreativeFragmentSettings creativeFragmentSettings)
		{
			_creativeFragmentSettings = creativeFragmentSettings;
			_serviceSettings = serviceSettings;
			_creativeEndpointsSettings = creativeEndpointsSettings;
			_documentStore = documentStore;
		}

		public override void Execute(CreateCreativeFragmentsTask task)
		{
			using (var session = _documentStore.OpenSession())
			{
				var creative = session.Load<Creative>(task.CreativeId);
				var unsubscribeTempalte = session.Load<Template>(creative.UnsubscribeTemplateId);

				foreach (var listId in creative.Lists)
				{
					var counter = 0;
					var chunk = _creativeFragmentSettings.RecipientsPerFragment;
					var hasMoreContacts = true;
					while (hasMoreContacts)
					{
						var id = listId;
						var contacts = session.Query<Contact>()
							.Customize(x => x.WaitForNonStaleResults())
							.Where(contact => contact.MemberOf.Any(x => x == id))
							.Skip(counter * chunk).Take(chunk).ToList();

						if (!contacts.Any())
						{
							hasMoreContacts = false;
							continue;
						}
						counter++;

						var recipients = contacts.Select(ToRecipient).ToList();
						ApplyDefaultRules(recipients);
						ApplyIntervalRules(recipients);

						var fragment = new CreativeFragment
						{
							Body = creative.Body,
							CreativeId = creative.Id,
							Subject = creative.Subject,
							Recipients = recipients,
							UnsubscribeTemplate = unsubscribeTempalte.Body,
							FromAddressDomainPrefix = creative.FromAddressDomainPrefix,
							FromName = creative.FromName,
							Service = new Core.Domain.Master.Service
										  {
											  BaseUrl = _serviceSettings.BaseUrl,
											  DealsEndpoint = _creativeEndpointsSettings.Deal,
											  UnsubscribeEndpoint = _creativeEndpointsSettings.Unsubscribe
										  }
						};
						session.Store(fragment);
						session.SaveChanges();
					}
				}

			}
		}

		private Recipient ToRecipient(Contact contact)
		{
			return new Recipient
						   {
							   Email = contact.Email,
							   Name = contact.Name,
							   ContactId = contact.Id
						   };
		}

		private void ApplyDefaultRules(IEnumerable<Recipient> recipients)
		{
			recipients
				.ToList()
				.ForEach(x =>
							 {
								 x.Interval = _creativeFragmentSettings.DefaultInterval;
								 x.Group = _creativeFragmentSettings.DefaultGroup;
							 });
		}

		private void ApplyIntervalRules(IEnumerable<Recipient> recipients)
		{
			using (var session = _documentStore.OpenSession())
			{
				var rules = session.Query<IntervalRule>().ToList();
				var matchingConditionsActions = rules.SelectMany(x => x.Conditons.Select(condition => new { Condition = condition, x.Interval, x.Group })).ToList();

				recipients
					.ToList()
					.ForEach(x => matchingConditionsActions
									  .Where(condition => x.Email.Contains(condition.Condition))
									  .ToList()
									  .ForEach(action =>
												   {
													   x.Interval = action.Interval;
													   x.Group = action.Group;
												   }));
			}
		}
	}
}