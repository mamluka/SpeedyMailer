using System.Collections.Generic;
using Raven.Client;
using System.Linq;
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
                email = GiveIDToEmail(email);
                session.Load<Email>(email.Id);

                session.Store(email);

                session.SaveChanges();
            }
        }

        private Email GiveIDToEmail(Email email)
        {
            email.Id = email.Address;
            return email;
        }

        public void Store(List<Email> emails)
        {
            using (var session = store.OpenSession())
            {
                emails = emails.Select(GiveIDToEmail).ToList();
                emails.ForEach(session.Store);

                session.SaveChanges();
            }
        }
    }
}