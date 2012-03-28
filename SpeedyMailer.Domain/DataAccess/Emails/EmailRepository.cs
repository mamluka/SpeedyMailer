using Raven.Client;
using SpeedyMailer.Domain.Model.Emails;

namespace SpeedyMailer.Domain.DataAccess.Emails
{
    public class EmailRepository : IEmailRepository
    {
        private readonly IDocumentStore store;
        private readonly IEmailSourceParser parser;

        public EmailRepository(IDocumentStore store, IEmailSourceParser parser)
        {
            this.store = store;
            this.parser = parser;
        }

        public void Store(Email email)
        {
            using (var session = store.OpenSession())
            {
                email.Deals = parser.Deals(email.Body);
                email.Id = string.Empty;

                session.Store(email);
                session.SaveChanges();
            }
        }
    }
}