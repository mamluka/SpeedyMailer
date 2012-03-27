using System.Collections.Generic;
using SpeedyMailer.Domain.Model.Contacts;

namespace SpeedyMailer.Domain.DataAccess.Contacts
{
    public interface IContactsRepository
    {
        void Store(Contact contact);
        void Store(List<Contact> emails);
        IEnumerable<Contact> GetContactsByListId(string listid, int whichPage, int howManyPerPage);
    }
}