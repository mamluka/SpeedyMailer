using System;
using Raven.Client;

namespace SpeedyMailer.Core.Emails
{
    public class EmailRepository
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
                email.Id = String.Empty;
                email.Deals = parser.Deals(email.Body);

                session.Store(email);
                session.SaveChanges();
            }
        }
    }
}