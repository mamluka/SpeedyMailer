using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Domain.Model.Contacts;
using SpeedyMailer.Domain.Model.Emails;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;
using System.Linq;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailPoolServicesTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void AddEmail_ShouldWriteTheFirstEmailFragmentWithContactsFromTheFirstPage   ()
        {
            //Arrange
            

            var email = Fixture.Build<Email>().With(x=> x.ToLists,new List<string> { "testlist"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Body == email.Body &&
                                                                   m.Subject == email.Subject &&
                                                                   TestIfRecipientsDataContainsTheContacts(contacts, m.ExtendedRecipients)
                                            ))).Repeat.Any();

            AddEmailBodyTemplateToSession(session);

            var store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            var emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }

        

        [Test]
        public void AddEmail_ShouldNotLockTheEmailFragmentWhenItIsFirstWritten()
        {
            //Arrange


            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist" }).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Locked==false
                                            ))).Repeat.Once();

            AddEmailBodyTemplateToSession(session);

            var store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            var emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }
        
        [Test]
        public void AddEmail_ShouldWriteTheFirstEmailFragmentWithTheContactsOfTheFirstPageInTheContactsList()
        {
            //Arrange


            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist" }).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Body == email.Body &&
                                                                   m.Subject == email.Subject &&
                                                                   TestIfRecipientsDataContainsTheContacts(contacts.Take(1000).ToList(), m.ExtendedRecipients)
                                            ))).Repeat.Any();

            AddEmailBodyTemplateToSession(session);

            var store = DocumentStoreFactory.StubDocumentStoreWithSession(session);


            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            var emailPool = componentBuilder
                .AddMockedContacts(contacts.Skip(0).Take(1000).ToList())
                .AddMockedContacts(contacts.Skip(1000).Take(1000).ToList())
                .FinishMockedContactlist()
                .Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldReturnInResultsTheNumberOfContactsUsed()
        {
            //Arrange


            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist" }).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateStub<IDocumentSession>();

            AddEmailBodyTemplateToSession(session);
           
            var store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            var emailPool = componentBuilder
                .AddMockedContacts(contacts.Skip(0).Take(1000).ToList())
                .AddMockedContacts(contacts.Skip(1000).Take(1000).ToList())
                .FinishMockedContactlist()
                .Build();
            //Act
            var results = emailPool.AddEmail(email);
            //Assert
            results.TotalNumberOfContacts.Should().Be(contacts.Count);
            results.TotalNumberOfEmailFragments.Should().Be((int) Math.Ceiling((decimal) (contacts.Count/1000.0)));
        }

        [Test]
        public void AddEmail_ShouldUseAllTheToListsGivenByTheEmail()
        {
            //Arrange

            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist1","testlist2" }).CreateAnonymous();

            var contactsRepository = MockRepository.GenerateMock<IContactsRepository>();

            contactsRepository
                .Expect(
                    x =>
                    x.GetContactsByListId(Arg<string>.Is.Equal("testlist1"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    new List<Contact>()
                ).Repeat.Any();

            contactsRepository
                .Expect(
                    x =>
                    x.GetContactsByListId(Arg<string>.Is.Equal("testlist2"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                   new List<Contact>()
                ).Repeat.Any();


            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.ContactsRepository = contactsRepository;

            var emailPool = componentBuilder.Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            contactsRepository.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldCallTheUrlCreatorWithTheRightEmailAddressAndEmailIdForTheDealUrl()
        {
            //Arrange
            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist1" }).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 1);

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x =>
                x.UrlByRouteWithJsonObject(Arg<string>.Is.Equal("Deals"),
                                           Arg<LeadIdentity>.Matches(
                                               m => m.Address == contacts[0].Address && m.EmailId == email.Id))).Repeat.Once().Return("url");

            var componenBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componenBuilder.UrlCreator = urlCreator;
            
            var emailPool = componenBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            urlCreator.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldCallTheUrlCreatorWithTheRightEmailAddressAndEmailIdforTheUnsubscribeUrl()
        {
            //Arrange
            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist1" }).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 1);

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x =>
                x.UrlByRouteWithJsonObject(Arg<string>.Is.Equal("Unsubscribe"),
                                           Arg<LeadIdentity>.Matches(
                                               m => m.Address == contacts[0].Address && m.EmailId == email.Id))).Repeat.Once().Return("url");

            var componenBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componenBuilder.UrlCreator = urlCreator;

            var emailPool = componenBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            urlCreator.VerifyAllExpectations();
        }

        [Test] public void AddEmail_ShouldStoreTheDealAndUnsubscribeUrls()
        {
            //Arrange
            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist" }).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 1);

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x =>
                x.UrlByRouteWithJsonObject(Arg<string>.Is.Equal("Deals"),
                                           Arg<LeadIdentity>.Is.Anything)).Repeat.Once().Return("dealUrl");

            urlCreator.Expect(
                x =>
                x.UrlByRouteWithJsonObject(Arg<string>.Is.Equal("Unsubscribe"),
                                           Arg<LeadIdentity>.Is.Anything)).Repeat.Once().Return("unsubscribeUrl");


            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.ExtendedRecipients[0].DealUrl == "dealUrl" &&
                                                                   m.ExtendedRecipients[0].UnsubscribeUrl == "unsubscribeUrl"
                                            ))).Repeat.Once();

            AddEmailBodyTemplateToSession(session);

            var store = DocumentStoreFactory.StubDocumentStoreWithSession(session);



            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;
            componentBuilder.UrlCreator = urlCreator;


            var emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
           
            session.VerifyAllExpectations();
            

        }

        [Test]
        public void AddEmail_ShouldLoadTheUnsubscribeEmailTemplateFromTheStoreAndSaveIfToTheFragment()
        {
            //Arrange


            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist" }).CreateAnonymous();

            var template = new EmailBodyElements()
                               {
                                   Unsubscribe = "template"
                               };

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.UnsubscribeTemplate == template.Unsubscribe
                                            ))).Repeat.Once();

            session.Expect(x => x.Load<EmailBodyElements>("system/templates")).Repeat.Once().Return(template);

            var store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            var emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();

        }

        private bool TestIfRecipientsDataContainsTheContacts(List<Contact> contacts, List<ExtendedRecipient> recipientDatas)
        {

            return recipientDatas.Select(x => x.Address).ToList() == contacts.Select(x => x.Address).ToList();

        }

        private void AddEmailBodyTemplateToSession(IDocumentSession session)
        {
            var template = new EmailBodyElements()
            {
                Unsubscribe = "template"
            };

            session.Expect(x => x.Load<EmailBodyElements>("system/templates")).Repeat.Once().Return(template);
        }

       
    }

    public class EmailPoolMockedComponentBuilder:IMockedComponentBuilder<EmailPoolService>
    {
        public IDocumentStore Store { get; set; }
        public IContactsRepository ContactsRepository { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IUrlCreator UrlCreator { get; set; }

        public EmailPoolMockedComponentBuilder(IMappingEngine mapper)
        {
            Mapper = mapper;
            var session = MockRepository.GenerateStub<IDocumentSession>();
            session.Stub(x => x.Load<EmailBodyElements>(Arg<string>.Is.Anything)).Repeat.Once().Return(
                new EmailBodyElements());

            Store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            ContactsRepository = MockRepository.GenerateMock<IContactsRepository>();
            UrlCreator = MockRepository.GenerateStub<IUrlCreator>();
        }

        
        public EmailPoolMockedComponentBuilder AddMockedContacts(List<Contact> contacts)
        {
            ContactsRepository.Stub(
                x => x.GetContactsByListId(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Repeat.
                Once().Return(contacts);

            return this;
        }

        public EmailPoolMockedComponentBuilder FinishMockedContactlist()
        {
            ContactsRepository.Stub(
                x => x.GetContactsByListId(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Repeat.
                Once().Return(new List<Contact>());

            return this;
        }



        public EmailPoolService Build()
        {
            return new EmailPoolService(Store, ContactsRepository, UrlCreator, Mapper);
        }
    }
}