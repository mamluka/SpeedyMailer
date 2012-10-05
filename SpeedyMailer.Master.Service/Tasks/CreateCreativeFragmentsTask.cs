using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using System.Linq;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;

namespace SpeedyMailer.Master.Service.Tasks
{
    public class CreateCreativeFragmentsTask : PersistentTask
    {
        public string CreativeId { get; set; }
        public int RecipientsPerFragment { get; set; }
    }

    public class CreateCreativeFragmentsTaskExecutor : PersistentTaskExecutor<CreateCreativeFragmentsTask>
    {
        private readonly IDocumentStore _documentStore;
	    private CreativeEndpointsSettings _creativeEndpointsSettings;
	    private ServiceSettings _serviceSettings;

	    public CreateCreativeFragmentsTaskExecutor(IDocumentStore documentStore,CreativeEndpointsSettings creativeEndpointsSettings, ServiceSettings serviceSettings)
	    {
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
                    var chunk = task.RecipientsPerFragment;
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

                        var fragment = new CreativeFragment
                        {
                            Body = creative.Body,
                            CreativeId = creative.Id,
                            Subject = creative.Subject,
                            Recipients = contacts,
                            UnsubscribeTemplate = unsubscribeTempalte.Body,
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
    }
}