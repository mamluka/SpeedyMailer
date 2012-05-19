using System.Web;
using AutoMapper;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Utilities.Domain.Contacts;

namespace SpeedyMailer.Core.Tests.Unit.Contacts
{
    public class ContactsCSVParserBuilder
    {
        private readonly IMappingEngine mapper;
        private IContactsRepository contactRepsitory;
        private HttpContextBase httpContext;

        public ContactsCSVParserBuilder(IMappingEngine mapper)
        {
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileStream(ContactCSVParserTests.GetDataStream());

            httpContext = httpContextBuilder.Build();

            contactRepsitory = MockRepository.GenerateStub<IContactsRepository>();
            this.mapper = mapper;
        }

        public HttpContextBase HttpContext
        {
            set { httpContext = value; }
        }

        public IContactsRepository ContactRepsitory
        {
            set { contactRepsitory = value; }
        }

        public ContactsCSVParser Build()
        {
            return new ContactsCSVParser(httpContext, mapper);
        }
    }
}