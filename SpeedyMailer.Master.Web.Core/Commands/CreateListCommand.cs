using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Lists;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class CreateListCommand : Command<string>
    {
        private readonly IDocumentStore _store;
        public string Name { get; set; }
        public int Id { get; set; }

        public CreateListCommand(IDocumentStore store)
        {
            _store = store;
        }

        public override string Execute()
        {
            using (var session = _store.OpenSession())
            {
                var listDescriptor = new ListDescriptor
                                         {
                                             Name = Name
                                         };
                session.Store(listDescriptor);
                session.SaveChanges();

                return listDescriptor.Id;
            }
        }
    }
}