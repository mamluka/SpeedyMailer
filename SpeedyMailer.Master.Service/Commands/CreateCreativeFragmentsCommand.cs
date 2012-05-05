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
        public string UnsubsribeTemplateId { get; set; }

        public CreateCreativeFragmentsCommand(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public override void Execute()
        {
            using (var session = _documentStore.OpenSession())
            {
                var creative = session.Load<Creative>(CreativeId);
                var unsubscribeTempalte = session.Load<CreativeTemplate>(UnsubsribeTemplateId);

                var recipients = creative.Lists.SelectMany(listId => session.Query<Contact>().Where(contact => contact.MemberOf.Any(x => x == listId))).ToList();
                var fragmentsChunks = recipients.Clump(RecipientsPerFragment);
                fragmentsChunks.ToList().ForEach(chunk=>
                                                     {
                                                         var fragment = new CreativeFragment
                                                                            {
                                                                                Creative = creative,
                                                                                Recipients = chunk.ToList(),
                                                                                UnsubscribeTemplate = unsubscribeTempalte.Body
                                                                            };
                                                         session.Store(fragment);
                                                     });

            }
        }
    }

    public class CreativeTemplate
    {
        public long Id { get; set; }
        public string Body { get; set; }
    }
}