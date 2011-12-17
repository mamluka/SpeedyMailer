using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Raven.Client;
using SpeedyMailer.Core.Contacts;

namespace SpeedyMailer.Core.Emails
{
    public class EmailPool:IEmailPool
    {
        private readonly IDocumentStore store;
        private readonly IContactsRepository contactsRepository;
        private readonly IMappingEngine mapper;

        public EmailPool(IDocumentStore store, IContactsRepository contactsRepository, IMappingEngine mapper)
        {
            this.store = store;
            this.contactsRepository = contactsRepository;
            this.mapper = mapper;
        }

        public AddEmailToPoolResults AddEmail(Email email)
        {
            using (var session = store.OpenSession())
            {
                var emailFragment = mapper.Map<Email, EmailFragment>(email);

                var totalContacts = 0;
                var totalFragments = 0;

                foreach (var list in email.ToLists)
                {
                    var pageNumber = 1;
                    
                    while (true)
                    {
                        var listFragment = contactsRepository.GetContactsByListID(list, pageNumber, 1000);
                        var currentListFragmentCount = listFragment.Count();
                        if ( currentListFragmentCount== 0)
                        {
                            break;
                        }

                        emailFragment.Recipients = (List<string>)listFragment;

                        session.Store(emailFragment);

                        session.SaveChanges();

                        totalContacts = totalContacts + currentListFragmentCount;

                        totalFragments++;

                        pageNumber++;
                    }
                }


                return new AddEmailToPoolResults()
                           {
                               TotalNumberOfContacts = totalContacts,
                               TotalNumberOfEmailFragments = totalFragments
                           };
            }
        }
    }
}