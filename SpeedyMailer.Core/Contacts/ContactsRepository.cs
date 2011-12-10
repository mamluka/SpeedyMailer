using System.Collections.Generic;
using Raven.Client;
using System.Linq;

namespace SpeedyMailer.Core.Contacts
{
    public class ContactsRepository : IContactsRepository
    {
        private readonly IDocumentStore store;

        public ContactsRepository(IDocumentStore store)
        {
            this.store = store;
        }


        public void Store(Contact contact)
        {
            using (var session = store.OpenSession())
            {
                contact = GiveIDToEmail(contact);
                session.Load<Contact>(contact.Id);

                session.Store(contact);

                session.SaveChanges();
            }
        }

        private Contact GiveIDToEmail(Contact contact)
        {
            contact.Id = contact.Address;
            return contact;
        }

        public void Store(List<Contact> emails)
        {
            using (var session = store.OpenSession())
            {
                emails = emails.Select(GiveIDToEmail).ToList();
                emails.ForEach(session.Store);

                session.SaveChanges();
            }
        }

        public List<string> GetContactsByListID(string listid, int whichPage, int howManyPerPage)
        {
            using (var session = store.OpenSession())
            {

            }
        }
    }
}