using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using SpeedyMailer.Domain.Model.Contacts;

namespace SpeedyMailer.Core.DataAccess.Contacts
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
				contact = GiveIdToContact(contact);
				session.Load<Contact>(contact.Id);

				session.Store(contact);

				session.SaveChanges();
			}
		}

		private Contact GiveIdToContact(Contact contact)
		{
			contact.Id = contact.Address;
			return contact;
		}

		public void Store(List<Contact> emails)
		{
			using (var session = store.OpenSession())
			{
				emails = emails.Select(GiveIdToContact).ToList();
				emails.ForEach(session.Store);

				session.SaveChanges();
			}
		}

		public IEnumerable<Contact> GetContactsByListId(string listid, int whichPage, int howManyPerPage)
		{
			using (var session = store.OpenSession())
			{
				return session.Query<Contact>().Where(x => x.MemberOf.Any(m => m == listid))
					.Skip((whichPage - 1)*howManyPerPage)
					.Take(howManyPerPage);

			}
		}
	}
}