using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;
using SpeedyMailer.Tests.Core.Unit.Database;

namespace SpeedyMailer.Core.Tests.Emails
{
    internal class EmailRepositoryBuilder : IMockedComponentBuilder<EmailRepository>
    {
        public IDocumentStore DocumentStore;
        public IEmailSourceParser Parser;

        public EmailRepositoryBuilder()
        {
            var session = MockRepository.GenerateStub<IDocumentSession>();
            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            DocumentStore = store;

            Parser = MockRepository.GenerateStub<IEmailSourceParser>();
        }


        public EmailRepository Build()
        {
            return new EmailRepository(DocumentStore, Parser);
        }

    }
}