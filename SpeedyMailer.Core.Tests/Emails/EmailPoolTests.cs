using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;
using System.Linq;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailPoolTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void AddEmail_ShouldWriteTheFirstEmailFragmentWithContactsFromTheFirstPage()
        {
            //Arrange
            

            var email = Fixture.Build<Email>().With(x=> x.ToLists,new List<string> { "testlist"}).CreateAnonymous();

            var contacts = new List<string>();
            contacts.AddMany(()=> Fixture.CreateAnonymous<string>(),25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Body == email.Body &&
                                                                   m.Subject == email.Subject &&
                                                                   m.Recipients == contacts
                                            ))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var contactsRepository = MockRepository.GenerateStub<IContactsRepository>();

            CreateStubForContactRepositoryToReturnContactsFromTheListNames(contacts, contactsRepository);


            var emailPool = new EmailPool(store,contactsRepository,Mapper);
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

            var contacts = new List<string>();
            contacts.AddMany(() => Fixture.CreateAnonymous<string>(), 25);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Locked==false
                                            ))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var contactsRepository = MockRepository.GenerateStub<IContactsRepository>();

            CreateStubForContactRepositoryToReturnContactsFromTheListNames(contacts, contactsRepository);


            var emailPool = new EmailPool(store, contactsRepository, Mapper);
            //Act
            emailPool.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }

        private static void CreateStubForContactRepositoryToReturnContactsFromTheListNames(List<string> contacts,IContactsRepository contactsRepository)
        {
            
            contactsRepository
                .Stub(x => x.GetContactsByListID(Arg<string>.Is.Equal("testlist"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    contacts
                ).Repeat.Once();

            contactsRepository
                .Stub(x => x.GetContactsByListID(Arg<string>.Is.Equal("testlist"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    new List<string>()
                ).Repeat.Once();
        }

        [Test]
        public void AddEmail_ShouldWriteTheFirstEmailFragmentWithTheContactsOfTheFirstPageInTheContactsList()
        {
            //Arrange


            var email = Fixture.Build<Email>().With(x => x.ToLists, new List<string> { "testlist" }).CreateAnonymous();

            var contacts = new List<string>();
            contacts.AddMany(() => Fixture.CreateAnonymous<string>(), 1700);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m =>
                                                                   m.Body == email.Body &&
                                                                   m.Subject == email.Subject &&
                                                                   m.Recipients == contacts.Take(1000).ToList()
                                            ))).Repeat.Any();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var contactsRepository = MockRepository.GenerateStub<IContactsRepository>();
            MockContactRepositoryWithTwoPagedList(contacts, contactsRepository);


            var emailPool = new EmailPool(store, contactsRepository, Mapper);
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

            var contacts = new List<string>();
            contacts.AddMany(() => Fixture.CreateAnonymous<string>(), 1700);

            var session = MockRepository.GenerateStub<IDocumentSession>();
           
            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var contactsRepository = MockRepository.GenerateStub<IContactsRepository>();
            MockContactRepositoryWithTwoPagedList(contacts, contactsRepository);


            var emailPool = new EmailPool(store, contactsRepository, Mapper);
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

            var contacts = new List<string>();
            contacts.AddMany(() => Fixture.CreateAnonymous<string>(), 1700);

            var session = MockRepository.GenerateStub<IDocumentSession>();
            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var contactsRepository = MockRepository.GenerateMock<IContactsRepository>();

            contactsRepository
                .Expect(
                    x =>
                    x.GetContactsByListID(Arg<string>.Is.Equal("testlist1"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    new List<string>()
                ).Repeat.Any();

            contactsRepository
                .Expect(
                    x =>
                    x.GetContactsByListID(Arg<string>.Is.Equal("testlist2"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    new List<string>()
                ).Repeat.Any();


            var emailPool = new EmailPool(store, contactsRepository, Mapper);
            //Act
            emailPool.AddEmail(email);
            //Assert
            contactsRepository.VerifyAllExpectations();
        }

        private static void MockContactRepositoryWithTwoPagedList(List<string> contacts,
                                                                            IContactsRepository contactsRepository)
        {

            contactsRepository
                .Stub(x => x.GetContactsByListID(Arg<string>.Is.Equal("testlist"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    contacts.Take(1000).ToList()
                ).Repeat.Once();

            contactsRepository
                .Stub(x => x.GetContactsByListID(Arg<string>.Is.Equal("testlist"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    contacts.Skip(1000).Take(1000).ToList()
                ).Repeat.Once();

            contactsRepository
                .Stub(x => x.GetContactsByListID(Arg<string>.Is.Equal("testlist"), Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(
                    new List<string>()
                ).Repeat.Once();
        }
    }
}