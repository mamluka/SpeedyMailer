using System.Collections.Generic;
using AutoMapper;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Domain.Contacts;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;

namespace SpeedyMailer.Core.Tests.Emails
{
    public class EmailPoolMockedComponentBuilder : IMockedComponentBuilder<EmailPoolService>
    {
        public EmailPoolMockedComponentBuilder(IMappingEngine mapper)
        {
            Mapper = mapper;
            var session = MockRepository.GenerateStub<IDocumentSession>();
            session.Stub(x => x.Load<EmailBodyElements>(Arg<string>.Is.Anything)).Repeat.Once().Return(
                new EmailBodyElements());

            Store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            ContactsRepository = MockRepository.GenerateMock<IContactsRepository>();
            UrlCreator = MockRepository.GenerateStub<IUrlCreator>();
        }

        public IDocumentStore Store { get; set; }
        public IContactsRepository ContactsRepository { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IUrlCreator UrlCreator { get; set; }


        public EmailPoolService Build()
        {
            return new EmailPoolService(Store, ContactsRepository, UrlCreator, Mapper);
        }


        public EmailPoolMockedComponentBuilder AddMockedContacts(List<Contact> contacts)
        {
            ContactsRepository.Stub(
                x => x.GetContactsByListId(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Repeat.
                Once().Return(contacts);

            return this;
        }

        public EmailPoolMockedComponentBuilder FinishMockedContactlist()
        {
            ContactsRepository.Stub(
                x => x.GetContactsByListId(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Repeat.
                Once().Return(new List<Contact>());

            return this;
        }
    }
}