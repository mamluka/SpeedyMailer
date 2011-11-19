using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailsRepositoryTests:AutoMapperAndFixtureBase
    {

        [Test]
        public void Store_ShouldStoreTheEmail()
        {
            //Arrange
            var email = Fixture.Build<Email>().Without(x => x.Id).CreateAnonymous();
            var store = MockRepository.GenerateStub<IDocumentStore>();

            var session = MockRepository.GenerateMock<IDocumentSession>();

            store.Expect(x => x.OpenSession()).Return(session);

            session.Expect(x => x.Store(Arg<Email>.Is.Equal(email))).Repeat.Once();

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

            var store = MockRepository.GenerateStub<IDocumentStore>();

            var session = MockRepository.GenerateMock<IDocumentSession>();

            store.Expect(x => x.OpenSession()).Return(session);

            session.Expect(x => x.Store(Arg<List<Email>>.Is.Anything)).Repeat.Times(10);

            //Act
            var emailsRep = new EmailsRepository(store);
            //Assert

            emailsRep.Store(emails);

            session.VerifyAllExpectations();

        }

    }
}