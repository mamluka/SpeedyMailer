using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Tests.Core.Emails;
using SpeedyMailer.Tests.Core.Unit.Base;
using SpeedyMailer.Tests.Core.Unit.Database;

namespace SpeedyMailer.Core.Tests.Unit.Emails
{
    internal class EmailRepositoryTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Store_ShouldSaveTheEmailInTheStore()
        {
            //Arrange

            var email = Fixture.CreateAnonymous<Email>();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Email>.Is.Equal(email))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailRepositoryBuilder();
            componentBuilder.DocumentStore = store;

            EmailRepository emailRepository = componentBuilder.Build();
            //Act
            emailRepository.Store(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Store_ShouldSaveTheEmailWithAnEmptyIDField()
        {
            //Arrange

            var email = Fixture.CreateAnonymous<Email>();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Email>.Matches(m => m.Id == String.Empty))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var componentBuilder = new EmailRepositoryBuilder();
            componentBuilder.DocumentStore = store;

            EmailRepository emailRepository = componentBuilder.Build();
            //Act
            emailRepository.Store(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Store_ShouldParseTheDealsInTheEmailBody()
        {
            //Arrange
            string emailBody = EmailSourceFactory.StandardEmail();

            Email email = Fixture.Build<Email>().With(x => x.Body, emailBody).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(
                x => x.Store(Arg<Email>.Matches(m => m.Deals.Any(p => p == "http://www.usocreports.com/switch/aladdin")
                                 ))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var parser = MockRepository.GenerateStub<IEmailSourceParser>();
            parser.Stub(x => x.Deals(Arg<string>.Is.Anything)).Return(new List<string>
                                                                          {"http://www.usocreports.com/switch/aladdin"});

            var componentBuilder = new EmailRepositoryBuilder();
            componentBuilder.DocumentStore = store;
            componentBuilder.Parser = parser;

            EmailRepository emailRepository = componentBuilder.Build();
            //Act
            emailRepository.Store(email);
            //Assert
            session.VerifyAllExpectations();
        }
    }
}