using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using System.Linq;
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

        public CreateCreativeFragmentsTaskExecutor(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public override void Execute(CreateCreativeFragmentsTask task)
        {
            using (var session = _documentStore.OpenSession())
            {
                var creative = session.Load<Creative>(task.CreativeId);
                var unsubscribeTempalte = session.Load<CreativeTemplate>(creative.UnsubscribeTemplateId);

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
                            UnsubscribeTemplate = unsubscribeTempalte.Body
                        };
                        session.Store(fragment);
                        session.SaveChanges();
                    }
                }

            }
        }
    }
}