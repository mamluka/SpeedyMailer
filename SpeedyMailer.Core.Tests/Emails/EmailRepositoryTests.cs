using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Domain.DataAccess.Emails;
using SpeedyMailer.Domain.Model.Emails;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;
using Ploeh.AutoFixture;
using SpeedyMailer.Tests.Core.Emails;

namespace SpeedyMailer.Core.Tests.Emails
{
    class EmailRepositoryTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Store_ShouldSaveTheEmailInTheStore()
        {
            //Arrange

            var email = Fixture.CreateAnonymous<Email>();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<Email>.Is.Equal(email))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var componentBuilder = new EmailRepositoryBuilder();
            componentBuilder.DocumentStore = store;

            var emailRepository = componentBuilder.Build();
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
            session.Expect(x => x.Store(Arg<Email>.Matches(m=> m.Id == String.Empty))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var componentBuilder = new EmailRepositoryBuilder();
            componentBuilder.DocumentStore = store;

            var emailRepository = componentBuilder.Build();
            //Act
            emailRepository.Store(email);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Store_ShouldParseTheDealsInTheEmailBody()
        {
            //Arrange
            var emailBody = EmailSourceFactory.StandardEmail();

            var email = Fixture.Build<Email>().With(x=> x.Body,emailBody).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(
                x => x.Store(Arg<Email>.Matches(m => m.Deals.Any(p => p == "http://www.usocreports.com/switch/aladdin")
                                 ))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var parser = MockRepository.GenerateStub<IEmailSourceParser>();
            parser.Stub(x => x.Deals(Arg<string>.Is.Anything)).Return(new List<string>
                                                                          {"http://www.usocreports.com/switch/aladdin"});

            var componentBuilder = new EmailRepositoryBuilder();
            componentBuilder.DocumentStore = store;
            componentBuilder.Parser = parser;

            var emailRepository = componentBuilder.Build();
            //Act
            emailRepository.Store(email);
            //Assert
            session.VerifyAllExpectations();

        }
    }

    class EmailRepositoryBuilder:IMockedComponentBuilder<EmailRepository>
    {
        public IDocumentStore DocumentStore;
        public IEmailSourceParser Parser;

        public EmailRepositoryBuilder()
        {
            var session = MockRepository.GenerateStub<IDocumentSession>();
            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            DocumentStore = store;

            Parser = MockRepository.GenerateStub<IEmailSourceParser>();
        }

        public EmailRepository Build()
        {
            return new EmailRepository(DocumentStore,Parser);
        }
    }
}
