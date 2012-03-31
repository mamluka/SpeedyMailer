using System.Collections.Generic;
using SpeedyMailer.Domain.Contacts;

namespace SpeedyMailer.Core.DataAccess.Contacts
{
    public interface IContactsRepository
    {
        void Store(Contact contact);
        void Store(List<Contact> emails);
        IEnumerable<Contact> GetContactsByListId(string listid, int whichPage, int howManyPerPage);
    }
}