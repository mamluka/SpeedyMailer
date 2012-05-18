using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using System.Linq;
using SpeedyMailer.Core.Utilities.Extentions;

namespace SpeedyMailer.Master.Service.Commands
{
    public class CreateCreativeFragmentsCommand:Command
    {
        private readonly IDocumentStore _documentStore;

        public string CreativeId { get; set; }
        public int RecipientsPerFragment { get; set; }

        public CreateCreativeFragmentsCommand(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public override void Execute()
        {
            using (var session = _documentStore.OpenSession())
            {
                var creative = session.Load<Creative>(CreativeId);
                var unsubscribeTempalte = session.Load<CreativeTemplate>(creative.UnsubscribeTemplateId);

                foreach (var listId in creative.Lists)
                {
                    var counter = 0;
                    var chunk = RecipientsPerFragment;
                    var hasMoreContacts = true;
                    while (hasMoreContacts)
                    {
                        var id = listId;
                        var contacts = session.Query<Contact>()
                            .Customize(x=> x.WaitForNonStaleResults())
                            .Where(contact => contact.MemberOf.Any(x => x == id))
                            .Skip(counter*chunk).Take(chunk).ToList();

                        if (!contacts.Any())
                        {
                            hasMoreContacts = false;
                            continue;
                        }
                        counter++;

                        var fragment = new CreativeFragment
                        {
                            Creative = creative,
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