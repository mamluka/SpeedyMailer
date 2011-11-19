using System.Collections.Generic;
using Raven.Client;

namespace SpeedyMailer.Core.Emails
{
    public class EmailsRepository : IEmailsRepository
    {
        private readonly IDocumentStore store;

        public EmailsRepository(IDocumentStore store)
        {
            this.store = store;
        }


        public void Store(Email email)
        {
            using (var session = store.OpenSession())
            {
                session.Store(email);

                session.SaveChanges();
            }
        }

        public void Store(List<Email> emails)
        {
            using (var session = store.OpenSession())
            {
                emails.ForEach(session.Store);

                session.SaveChanges();
            }
        }
    }
}