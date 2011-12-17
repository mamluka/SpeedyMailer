using System.Collections.Generic;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;

namespace SpeedyMailer.Core.Tests.Contacts
{
    [TestFixture]
    public class ContactsRepositoryTests:AutoMapperAndFixtureBase<AutoMapperMaps>
    {

        [Test]
        public void Store_ShouldStoreTheContact()
        {
            //Arrange
            var contact = Fixture.Build<Contact>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Contact>.Is.Equal(contact))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            //Act
            var contactsRep = new ContactsRepository(store);
            //Assert

            contactsRep.Store(contact);

            session.VerifyAllExpectations();

        }

        [Test]
        public void Store_ShouldGiveTheContactIDSameAsTheContactAddress()
        {
            //Arrange
            var contact = Fixture.Build<Contact>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Contact>.Matches(m=> m.Id == contact.Address))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

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
            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var contactsRep = new ContactsRepository(store);

            //Act
             contactsRep.Store(contacts);   
            //Assert

           

            session.VerifyAllExpectations();

        }

       
      
    }
}