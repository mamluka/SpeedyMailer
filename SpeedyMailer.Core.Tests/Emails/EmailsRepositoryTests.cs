using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailsRepositoryTests:AutoMapperAndFixtureBase<AutoMapperMaps>
    {

        [Test]
        public void Store_ShouldStoreTheEmail()
        {
            //Arrange
            var email = Fixture.Build<Email>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Email>.Is.Equal(email))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            //Act
            var emailsRep = new EmailsRepository(store);
            //Assert

            emailsRep.Store(email);

            session.VerifyAllExpectations();

        }

        [Test]
        public void Store_ShouldGiveTheEmailIDSameAsTheEmailAddress()
        {
            //Arrange
            var email = Fixture.Build<Email>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Email>.Matches(m=> m.Id == email.Address))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            //Act
            var emailsRep = new EmailsRepository(store);
            //Assert

            emailsRep.Store(email);

            session.VerifyAllExpectations();

        }

        [Test]
        public void Store_ShouldStoreAListOfEmailsVerifyThatMethodCalled10Times()
        {
            //Arrange
            var emails = new List<Email>();

            emails.AddMany(() => Fixture.Build<Email>().Without(x => x.Id).CreateAnonymous(), 10);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<List<Email>>.Is.Anything)).Repeat.Times(10);
            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);



            //Act
            var emailsRep = new EmailsRepository(store);
            //Assert

            emailsRep.Store(emails);

            session.VerifyAllExpectations();

        }

        [Test]
        public void Store_ShouldGiveAllTheEmailsTheIDOfTheCurrentEmail()
        {
            //Arrange
            var emails = new List<Email>();

            emails.AddMany(() => Fixture.Build<Email>().Without(x => x.Id).CreateAnonymous(), 1);

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Email>.Matches(m=> m.Id == emails[0].Address))).Repeat.Once();
            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);



            //Act
            var emailsRep = new EmailsRepository(store);
            //Assert

            emailsRep.Store(emails);

            session.VerifyAllExpectations();

        }
    }
}