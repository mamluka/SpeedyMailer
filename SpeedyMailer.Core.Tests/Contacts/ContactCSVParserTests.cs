using System.Collections.Generic;
using System.IO;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using System.Linq;

namespace SpeedyMailer.Core.Tests.Contacts
{
    [TestFixture]
    public class ContactCSVParserTests:AutoMapperAndFixtureBase<AutoMapperMaps>
    {
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

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();
            //Assert
            httpRequest.VerifyAllExpectations();

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

            var parser = parserBuilder.Build();

            //Act
            parser.ParseAndStore();
            //Assert
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

            var parser = parserBuilder.Build();

            //Act
            parser.ParseAndStore();
            //Assert
        }

        private static MemoryStream EmptyFile()
        {
            return new MemoryStream();
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

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();
            //Assert
            file.VerifyAllExpectations();
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
            contactsRepository.Expect(x => x.Store(Arg<List<Contact>>.Matches(m=> m.All(p=> p.MemberOf.Contains(model.ContainingListId))))).Repeat.Once();

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.ContactRepsitory = contactsRepository;


            
            var parser = parserBuilder.Build();
            parser.AddInitialContactBatchOptions(model);
            //Act
            parser.ParseAndStore();


            //Assert
            contactsRepository.VerifyAllExpectations();

        }

        [Test]
        public void ParseAndSave_ShouldCallTheStoreMethodOnTheRepositoryWithTheList()
        {
            //Arrange
            var contactsRepository = MockRepository.GenerateMock<IContactsRepository>();
            contactsRepository.Expect(x => x.Store(Arg<List<Contact>>.Is.Anything)).Repeat.Once();

            var parserBuilder = new ContactsCSVParserBuilder(Mapper);
            parserBuilder.ContactRepsitory = contactsRepository;

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

    
            //Assert
            contactsRepository.VerifyAllExpectations();

        }

        [Test]
        public void Results_ShouldReturnTheNumberOfFilesProcessed()
        {
            //Arrange
            var parserBuilder = new ContactsCSVParserBuilder(Mapper);

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            var results = parser.Results;
            //Assert
            results.NumberOfFilesProcessed.Should().Be(1);

        }

        [Test]
        public void Results_ShouldReturnTheNumberOfContactsProcessed()
        {
            //Arrange
            var parserBuilder = new ContactsCSVParserBuilder(Mapper);

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            var results = parser.Results;
            //Assert
            results.NumberOfContactsProcessed.Should().Be(5);

        }

        [Test]
        public void Results_ShouldReturnTheNamesOFTheFilesThatWereProcessed()
        {
            //Arrange
            var parserBuilder = new ContactsCSVParserBuilder(Mapper);

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            var results = parser.Results;
            //Assert
            results.Filenames.Should().HaveCount(1);
            results.Filenames.Should().Contain("contacts.csv");

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

       


        

    }

    public class ContactsCSVParserBuilder
    {
        private HttpContextBase httpContext;
        private IContactsRepository contactRepsitory;
        private IMappingEngine mapper;

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
            return new ContactsCSVParser(httpContext, contactRepsitory, mapper);
        }

        
    }

    public class HttpContextBaseBuilderForFiles
    {
        private int fileCount;
        private Stream fileStream;
        private HttpContextBase httpContext;
        private HttpRequestBase httpRequest;
        private HttpFileCollectionBase files;
        private HttpPostedFileBase file;
        private string fileName;

        public HttpFileCollectionBase Files
        {
            get
            {
                return files;
            }
            set { files = value; }
        }

        public HttpContextBaseBuilderForFiles()
        {
            fileCount = 1;
            fileName = "contacts.csv";
            fileStream = new MemoryStream();
            httpContext = MockRepository.GenerateStub<HttpContextBase>();
            httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
            Files = MockRepository.GenerateStub<HttpFileCollectionBase>();
            file = MockRepository.GenerateStub<HttpPostedFileBase>();
        }

       

        public HttpContextBaseBuilderForFiles WithFileCount(int fileCount)
        {
            this.fileCount = fileCount;
            return this;
        }

        public HttpContextBaseBuilderForFiles WithFileStream(Stream stream)
        {
            fileStream = stream;
            return this;
        }

        public HttpContextBaseBuilderForFiles WithFileName(string fileName)
        {
            this.fileName = fileName;
            return this;
        }

        public HttpContextBase Build()
        {
            httpContext.Stub(x => x.Request).Return(httpRequest);
            BuildFiles();
            httpRequest.Stub(x => x.Files).Return(Files);

            return httpContext;
        }

        public HttpContextBaseBuilderForFiles BuildFiles()
        {
            Files.Stub(x => x.Count).Return(fileCount);

            Files.Stub(x => x[0]).Return(file);

            file.Stub(x => x.InputStream).Return(fileStream);
            file.Stub(x => x.FileName).Return(fileName);

            return this;
        }


        public HttpContextBaseBuilderForFiles Replace(HttpRequestBase replacingHttpRequest)
        {
            httpRequest = replacingHttpRequest;
            return this;
        }

        public void Replace(HttpPostedFileBase replacingHttpPostedFile)
        {
            file = replacingHttpPostedFile;
        }
    }
}