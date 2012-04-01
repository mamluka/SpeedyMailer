using Raven.Client;
using SpeedyMailer.Domain.Emails;

namespace SpeedyMailer.Core.DataAccess.Emails
{
    public class EmailRepository : IEmailRepository
    {
        private readonly IEmailSourceParser parser;
        private readonly IDocumentStore store;

        public EmailRepository(IDocumentStore store, IEmailSourceParser parser)
        {
            this.store = store;
            this.parser = parser;
        }


        public void Store(Email email)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                email.Deals = parser.Deals(email.Body);
                email.Id = string.Empty;

                session.Store(email);
                session.SaveChanges();
            }
        }

    }
}