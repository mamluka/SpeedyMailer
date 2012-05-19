using System.Collections.Generic;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Tests.Core.Unit.Base;
using SpeedyMailer.Tests.Core.Unit.Database;

namespace SpeedyMailer.Core.Tests.Unit.Contacts
{
    [TestFixture]
    public class ContactsRepositoryTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Store_ShouldGiveTheContactIDSameAsTheContactAddress()
        {
            //Arrange
            Contact contact = Fixture.Build<Contact>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Contact>.Matches(m => m.Id == contact.Email))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            //Act
            var contactsRep = new ContactsRepository(store);
            //Assert

            contactsRep.Store(contact);

            session.VerifyAllExpectations();
        }

        [Test]
        public void Store_ShouldStoreAListOfContactsVerifyThatMethodCalled10Times()
        {
            //Arrange
            var contacts = new List<Contact>();

            contacts.AddMany(() => Fixture.Build<Contact>().Without(x => x.Id).CreateAnonymous(), 10);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<List<Contact>>.Is.Anything)).Repeat.Times(10);
            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var contactsRep = new ContactsRepository(store);

            //Act
            contactsRep.Store(contacts);
            //Assert


            session.VerifyAllExpectations();
        }

        [Test]
        public void Store_ShouldStoreTheContact()
        {
            //Arrange
            Contact contact = Fixture.Build<Contact>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Contact>.Is.Equal(contact))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            //Act
            var contactsRep = new ContactsRepository(store);
            //Assert

            contactsRep.Store(contact);

            session.VerifyAllExpectations();
        }
    }
}