using Raven.Client;

namespace SpeedyMailer.Core.Emails
{
    public class EmailsRepository
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
    }
}