using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Helpers;

namespace SpeedyMailer.Core.Emails
{
    public class EmailPool:IEmailPool
    {
        private readonly IDocumentStore store;
        private readonly IContactsRepository contactsRepository;
        private readonly IUrlCreator urlCreator;
        private readonly IMappingEngine mapper;

        public EmailPool(IDocumentStore store, IContactsRepository contactsRepository, IUrlCreator urlCreator, IMappingEngine mapper)
        {
            this.store = store;
            this.contactsRepository = contactsRepository;
            this.urlCreator = urlCreator;
            this.mapper = mapper;
        }

        public AddEmailToPoolResults AddEmail(Email email)
        {
            using (var session = store.OpenSession())
            {
                var emailFragment = mapper.Map<Email, EmailFragment>(email);

                var totalContacts = 0;
                var totalFragments = 0;

                var unsubscribeTemplate = session.Load<EmailBodyElements>("system/templates");

                foreach (var list in email.ToLists)
                {
                    var pageNumber = 1;
                    
                    while (true)
                    {
                        var listFragment = contactsRepository.GetContactsByListId(list, pageNumber, 1000);
                        var currentListFragmentCount = listFragment.Count();
                        if ( currentListFragmentCount== 0)
                        {
                            break;
                        }


                        emailFragment.UnsubscribeTemplate = unsubscribeTemplate.Unsubscribe ?? "";

                        emailFragment.ExtendedRecipients = listFragment.Select(x =>
                                                                               new ExtendedRecipient()
                                                                                   {
                                                                                       Address = x.Address,
                                                                                       Name = x.Name,
                                                                                       DealUrl = urlCreator.UrlByRouteWithJsonObject("Deals",new LeadIdentity()
                                                                                                                                                 {
                                                                                                                                                     Address = x.Address,
                                                                                                                                                     EmailId = email.Id
                                                                                                                                                 }),
                                                                                       UnsubscribeUrl = urlCreator.UrlByRouteWithJsonObject("Unsubscribe", new LeadIdentity()
                                                                                       {
                                                                                           Address = x.Address,
                                                                                           EmailId = email.Id
                                                                                       })

                                                                                       
                                                                                   }
                            ).ToList();


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

        public EmailFragment PopEmail()
        {
            using (var session = store.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;

                var emailFragment = session.Query<EmailFragment>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.Locked == false)
                    .OrderByDescending(x => x.CreateDate)
                    .Take(1)
                    .SingleOrDefault();

                if (emailFragment != null)
                {
                    try
                    {
                    emailFragment.Locked = true;
                    
                        session.SaveChanges();
                        return emailFragment;
                    }
                    catch (ConcurrencyException)
                    {
                        return PopEmail();
                    }


                }
                    return null;
                
            }
        }
    }
}