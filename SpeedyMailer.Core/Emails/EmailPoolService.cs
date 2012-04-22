using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Raven.Client;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Utilities.Domain.Email;

namespace SpeedyMailer.Core.Emails
{
    public class EmailPoolService : IEmailPoolService
    {
        private readonly IContactsRepository contactsRepository;
        private readonly IMappingEngine mapper;
        private readonly IDocumentStore store;
        private readonly IUrlCreator urlCreator;

        public EmailPoolService(IDocumentStore store, IContactsRepository contactsRepository, IUrlCreator urlCreator,
                                IMappingEngine mapper)
        {
            this.store = store;
            this.contactsRepository = contactsRepository;
            this.urlCreator = urlCreator;
            this.mapper = mapper;
        }


        public AddEmailToPoolResults AddEmail(Email email)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                int totalContacts = 0;
                int totalFragments = 0;

                var systemTemplates = session.Load<EmailBodyElements>("system/templates");

                foreach (string list in email.ToLists)
                {
                    int pageNumber = 1;

                    while (true)
                    {
                        IEnumerable<Contact> listFragment = contactsRepository.GetContactsByListId(list, pageNumber,
                                                                                                   1000);
                        int currentListFragmentCount = listFragment.Count();
                        if (currentListFragmentCount == 0)
                        {
                            break;
                        }

                        EmailFragment emailFragment = mapper.Map<Email, EmailFragment>(email);

                        emailFragment.UnsubscribeTemplate = systemTemplates.Unsubscribe ?? "";

                        emailFragment.MailId = email.Id;

                        emailFragment.Locked = false;

                        emailFragment.ExtendedRecipients = listFragment.Select(x =>
                                                                               new ExtendedRecipient
                                                                                   {
                                                                                       Address = x.Address,
                                                                                       Name = x.Name,
                                                                                       DealUrl =
                                                                                           urlCreator.
                                                                                           UrlByRouteWithJsonObject(
                                                                                               "Deals",
                                                                                               new LeadIdentity
                                                                                                   {
                                                                                                       Address =
                                                                                                           x.Address,
                                                                                                       EmailId =
                                                                                                           email.Id
                                                                                                   }),
                                                                                       UnsubscribeUrl =
                                                                                           urlCreator.
                                                                                           UrlByRouteWithJsonObject(
                                                                                               "Unsubscribe",
                                                                                               new LeadIdentity
                                                                                                   {
                                                                                                       Address =
                                                                                                           x.Address,
                                                                                                       EmailId =
                                                                                                           email.Id
                                                                                                   })
                                                                                   }
                            ).ToList();


                        session.Store(emailFragment);


                        totalContacts = totalContacts + currentListFragmentCount;

                        totalFragments++;

                        pageNumber++;
                    }
                }

                session.SaveChanges();


                return new AddEmailToPoolResults
                           {
                               TotalNumberOfContacts = totalContacts,
                               TotalNumberOfEmailFragments = totalFragments
                           };
            }
        }

    }
}