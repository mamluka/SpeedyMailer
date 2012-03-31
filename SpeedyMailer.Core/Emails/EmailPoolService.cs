using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using AutoMapper;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Helpers;
using System.Configuration;
using SpeedyMailer.Domain.Model.Emails;

namespace SpeedyMailer.Core.Emails
{
    public class EmailPoolService:IEmailPoolService
    {
        private readonly IDocumentStore store;
        private readonly IContactsRepository contactsRepository;
        private readonly IUrlCreator urlCreator;
        private readonly IMappingEngine mapper;

        public EmailPoolService(IDocumentStore store, IContactsRepository contactsRepository, IUrlCreator urlCreator, IMappingEngine mapper)
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
                

                var totalContacts = 0;
                var totalFragments = 0;

                var systemTemplates = session.Load<EmailBodyElements>("system/templates");

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

                        var emailFragment = mapper.Map<Email, EmailFragment>(email);

                        emailFragment.UnsubscribeTemplate = systemTemplates.Unsubscribe ?? "";

                        emailFragment.MailId = email.Id;

                        emailFragment.Locked = false;

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

                        

                        totalContacts = totalContacts + currentListFragmentCount;

                        totalFragments++;

                        pageNumber++;
                    }
                }

                session.SaveChanges();


                return new AddEmailToPoolResults()
                           {
                               TotalNumberOfContacts = totalContacts,
                               TotalNumberOfEmailFragments = totalFragments
                           };
            }
        }

      
    }
}