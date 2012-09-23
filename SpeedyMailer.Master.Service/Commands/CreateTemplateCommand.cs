using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Service.Commands
{
    public class CreateTemplateCommand:Command<string>
    {
        private IDocumentStore _documentStore;
        public string Body { get; set; }

        public CreateTemplateCommand(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public override string Execute()
        {
            using (var session = _documentStore.OpenSession())
            {
                var template = new CreativeTemplate
                                   {
                                       Body = Body
                                   };

                session.Store(template);
                session.SaveChanges();
                return template.Id;
            }
        }
        
    }
}