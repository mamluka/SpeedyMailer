using System.Collections.Generic;

namespace SpeedyMailer.Domain.DataAccess.Contact
{
    public interface IContactsRepository
    {
        void Store(Contact contact);
        void Store(List<Contact> emails);
        IEnumerable<Contact> GetContactsByListId(string listid, int whichPage, int howManyPerPage);
    }
}