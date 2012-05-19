using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Utilities.Domain.Contacts;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.Tests.Unit.Contacts
{
    [TestFixture]
    public class ContactCSVParserTests : AutoMapperAndFixtureBase
    {
        private static MemoryStream EmptyFile()
        {
            return new MemoryStream();
        }

        public static MemoryStream GetDataStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("contact,name,country");
            writer.WriteLine("plxufab7@bootbox.net,John,Israel");
            writer.WriteLine("mark.morgan@clarkconstruction.com,David,USA");
            writer.WriteLine("joseph.morin@bellsouth.net,Stuar,Sweden");
            writer.WriteLine("richardduby63@moocow.com,Alex,Russia");
            writer.WriteLine("klcreepy@ptd.net,Bob,UK");

            writer.Flush();
            stream.Position = 0;

            return stream;
        }


        [Test]
        public void ParseAndSave_ShouldAddTheChoosenListFromTheModelToTheContact()
        {
            //Arrange

            var model = new InitialContactsBatchOptions
                            {
                                ContainingListId = "mycategpryid"
                            };


            var contactsRepository = MockRepository.GenerateMock<IContactsRepository>();
            contactsRepository.Expect(
                x => x.Store(Arg<List<Contact>>.Matches(m => m.All(p => p.MemberOf.Contains(model.ContainingListId))))).
                Repeat.Once();

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.ContactRepsitory = contactsRepository;


            ContactsCSVParser parser = parserBuilder.Build();
            parser.AddInitialContactBatchOptions(model);
            //Act
            parser.ParseAndStore();


            //Assert
            contactsRepository.VerifyAllExpectations();
        }

        [Test]
        public void ParseAndSave_ShouldCallTheFilesPropertyInTheRequestObject()
        {
            //Arrange
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileCount(1);

            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();

            httpRequest.Expect(x => x.Files).Repeat.Once().Return(httpContextBuilder.Files);

            httpContextBuilder.Replace(httpRequest);
            httpContextBuilder.WithFileStream(GetDataStream());

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.HttpContext = httpContextBuilder.Build();

            ContactsCSVParser parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();
            //Assert
            httpRequest.VerifyAllExpectations();
        }

        [Test]
        public void ParseAndSave_ShouldCallTheStoreMethodOnTheRepositoryWithTheList()
        {
            //Arrange
            var contactsRepository = MockRepository.GenerateMock<IContactsRepository>();
            contactsRepository.Expect(x => x.Store(Arg<List<Contact>>.Is.Anything)).Repeat.Once();

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.ContactRepsitory = contactsRepository;

            ContactsCSVParser parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();


            //Assert
            contactsRepository.VerifyAllExpectations();
        }

        [Test]
        public void ParseAndSave_ShouldReadTheInputStreamFromTheFile()
        {
            //Arrange
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileCount(1);

            var file = MockRepository.GenerateMock<HttpPostedFileBase>();
            file.Expect(x => x.InputStream).Return(GetDataStream());

            httpContextBuilder.Replace(file);

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.HttpContext = httpContextBuilder.Build();

            ContactsCSVParser parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();
            //Assert
            file.VerifyAllExpectations();
        }

        [Test]
        [ExpectedException]
        public void ParseAndSave_ShouldThrowAnExceptionIfTheFileIsEmpty()
        {
            //Arrange
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileCount(1).WithFileStream(EmptyFile());


            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.HttpContext = httpContextBuilder.Build();

            ContactsCSVParser parser = parserBuilder.Build();

            //Act
            parser.ParseAndStore();
            //Assert
        }

        [Test]
        [ExpectedException]
        public void ParseAndSave_ShouldThrowAnExceptionIfThereAreZeroFiles()
        {
            //Arrange
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileCount(0);

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.HttpContext = httpContextBuilder.Build();

            ContactsCSVParser parser = parserBuilder.Build();

            //Act
            parser.ParseAndStore();
            //Assert
        }

        [Test]
        public void Results_ShouldReturnTheNamesOFTheFilesThatWereProcessed()
        {
            //Arrange
            var parserBuilder = new ContactsCSVParserBuilder(Mapper);

            ContactsCSVParser parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            ContactCSVParserResults results = parser.Results;
            //Assert
            results.Filenames.Should().HaveCount(1);
            results.Filenames.Should().Contain("contacts.csv");
        }

        [Test]
        public void Results_ShouldReturnTheNumberOfContactsProcessed()
        {
            //Arrange
            var parserBuilder = new ContactsCSVParserBuilder(Mapper);

            ContactsCSVParser parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            ContactCSVParserResults results = parser.Results;
            //Assert
            results.NumberOfContactsProcessed.Should().Be(5);
        }

        [Test]
        public void Results_ShouldReturnTheNumberOfFilesProcessed()
        {
            //Arrange
            var parserBuilder = new ContactsCSVParserBuilder(Mapper);

            ContactsCSVParser parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            ContactCSVParserResults results = parser.Results;
            //Assert
            results.NumberOfFilesProcessed.Should().Be(1);
        }
    }
}