using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Utilities.Domain.Email;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;
using SpeedyMailer.Tests.Core.Unit.Database;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailPoolServicesTests : AutoMapperAndFixtureBase
    {
        private bool TestIfRecipientsDataContainsTheContacts(List<Contact> contacts,
                                                             List<ExtendedRecipient> recipientDatas)
        {
            return recipientDatas.Select(x => x.Address).ToList() == contacts.Select(x => x.Email).ToList();
        }

        private void AddEmailBodyTemplateToSession(IDocumentSession session)
        {
            var template = new EmailBodyElements
                               {
                                   Unsubscribe = "template"
                               };

            session.Expect(x => x.Load<EmailBodyElements>("system/templates")).Repeat.Once().Return(template);
        }

        [Test]
        public void AddEmail_ShouldCallTheUrlCreatorWithTheRightEmailAddressAndEmailIdForTheDealUrl()
        {
            //Arrange
            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist1"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 1);

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x =>
                x.UrlByRouteWithJsonObject(Arg<string>.Is.Equal("Deals"),
                                           Arg<LeadIdentity>.Matches(
                                               m => m.Address == contacts[0].Email && m.EmailId == email.Id))).Repeat.
                Once().Return("url");

            var componenBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componenBuilder.UrlCreator = urlCreator;

            EmailPoolService emailPool = componenBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            urlCreator.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldCallTheUrlCreatorWithTheRightEmailAddressAndEmailIdforTheUnsubscribeUrl()
        {
            //Arrange
            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist1"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 1);

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x =>
                x.UrlByRouteWithJsonObject(Arg<string>.Is.Equal("Unsubscribe"),
                                           Arg<LeadIdentity>.Matches(
                                               m => m.Address == contacts[0].Email && m.EmailId == email.Id))).Repeat.
                Once().Return("url");

            var componenBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componenBuilder.UrlCreator = urlCreator;

            EmailPoolService emailPool = componenBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            urlCreator.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldLoadTheUnsubscribeEmailTemplateFromTheStoreAndSaveIfToTheFragment()
        {
            //Arrange


            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist"}).CreateAnonymous();

            var template = new EmailBodyElements
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

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            EmailPoolService emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldNotLockTheEmailFragmentWhenItIsFirstWritten()
        {
            //Arrange


            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Locked == false
                                            ))).Repeat.Once();

            AddEmailBodyTemplateToSession(session);

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            EmailPoolService emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldReturnInResultsTheNumberOfContactsUsed()
        {
            //Arrange


            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateStub<IDocumentSession>();

            AddEmailBodyTemplateToSession(session);

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            EmailPoolService emailPool = componentBuilder
                .AddMockedContacts(contacts.Skip(0).Take(1000).ToList())
                .AddMockedContacts(contacts.Skip(1000).Take(1000).ToList())
                .FinishMockedContactlist()
                .Build();
            //Act
            AddEmailToPoolResults results = emailPool.AddEmail(email);
            //Assert
            results.TotalNumberOfContacts.Should().Be(contacts.Count);
            results.TotalNumberOfEmailFragments.Should().Be((int) Math.Ceiling((decimal) (contacts.Count/1000.0)));
        }

        [Test]
        public void AddEmail_ShouldStoreTheDealAndUnsubscribeUrls()
        {
            //Arrange
            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist"}).CreateAnonymous();

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
                                                                   m.ExtendedRecipients[0].UnsubscribeUrl ==
                                                                   "unsubscribeUrl"
                                            ))).Repeat.Once();

            AddEmailBodyTemplateToSession(session);

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);


            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;
            componentBuilder.UrlCreator = urlCreator;


            EmailPoolService emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert

            session.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldUseAllTheToListsGivenByTheEmail()
        {
            //Arrange

            Email email =
                Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist1", "testlist2"}).CreateAnonymous
                    ();

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

            EmailPoolService emailPool = componentBuilder.Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            contactsRepository.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldWriteTheFirstEmailFragmentWithContactsFromTheFirstPage()
        {
            //Arrange


            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Body == email.Body &&
                                                                   m.Subject == email.Subject &&
                                                                   TestIfRecipientsDataContainsTheContacts(contacts,
                                                                                                           m.
                                                                                                               ExtendedRecipients)
                                            ))).Repeat.Any();

            AddEmailBodyTemplateToSession(session);

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            EmailPoolService emailPool = componentBuilder.AddMockedContacts(contacts).FinishMockedContactlist().Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void AddEmail_ShouldWriteTheFirstEmailFragmentWithTheContactsOfTheFirstPageInTheContactsList()
        {
            //Arrange


            Email email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> {"testlist"}).CreateAnonymous();

            var contacts = new List<Contact>();
            contacts.AddMany(() => Fixture.CreateAnonymous<Contact>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Body == email.Body &&
                                                                   m.Subject == email.Subject &&
                                                                   TestIfRecipientsDataContainsTheContacts(
                                                                       contacts.Take(1000).ToList(),
                                                                       m.ExtendedRecipients)
                                            ))).Repeat.Any();

            AddEmailBodyTemplateToSession(session);

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);


            var componentBuilder = new EmailPoolMockedComponentBuilder(Mapper);
            componentBuilder.Store = store;


            EmailPoolService emailPool = componentBuilder
                .AddMockedContacts(contacts.Skip(0).Take(1000).ToList())
                .AddMockedContacts(contacts.Skip(1000).Take(1000).ToList())
                .FinishMockedContactlist()
                .Build();
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }
    }
}