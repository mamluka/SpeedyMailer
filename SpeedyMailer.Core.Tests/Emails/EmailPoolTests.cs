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
    public class EmailPoolTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void AddEmail_ShouldWriteAQueueToTheStore()
        {
            //Arrange
            var emailFragment = Fixture.CreateAnonymous<EmailFragment>();

            var email = Fixture.CreateAnonymous<Email>();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Is.Equal(emailFragment))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var emailPoos = new EmailPool(store);
            //Act
            emailPoos.AddEmail(email);
            //Assert
            session.VerifyAllExpectations();
        }
    }

    public class EmailFragment
    {
        public string Body { get; set; }
        public List<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string ReportURL { get; set; }
    }

    public class EmailPool:IEmailPool
    {
        private readonly IDocumentStore store;

        public EmailPool(IDocumentStore store)
        {
            this.store = store;
        }

        public AddEmailToPoolResults AddEmail(Email email)
        {
            using (var session = store.OpenSession())
            {
                return null;
            }
        }
    }
}