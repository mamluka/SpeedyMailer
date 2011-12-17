using System.Collections.Generic;

namespace SpeedyMailer.Core.Contacts
{
    public interface IContactsRepository
    {
        void Store(Contact contact);
        void Store(List<Contact> emails);
        IEnumerable<string> GetContactsByListID(string listid, int whichPage, int howManyPerPage);
    }
}