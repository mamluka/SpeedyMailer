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
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;
using Ploeh.AutoFixture;

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

            var emailRepository = new EmailRepository(store);
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

            var emailRepository = new EmailRepository(store);
            //Act
            emailRepository.Store(email);
            //Assert
            session.VerifyAllExpectations();
        }
    }

    public class EmailRepository
    {
        private readonly IDocumentStore store;

        public EmailRepository(IDocumentStore store)
        {
            this.store = store;
            
        }

        public void Store(Email email)
        {
            using (var session = store.OpenSession())
            {
                email.Id = String.Empty;
                session.Store(email);
            }
        }
    }
}
