using System.Collections.Generic;

namespace SpeedyMailer.Core.Contacts
{
    public interface IContactsRepository
    {
        void Store(Contact contact);
        void Store(List<Contact> emails);
        IEnumerable<Contact> GetContactsByListId(string listid, int whichPage, int howManyPerPage);
    }
}