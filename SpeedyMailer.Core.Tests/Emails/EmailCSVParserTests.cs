using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Core.Emails;
using AutoMapper;


namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailCSVParserTests:AutoMapperAndFixtureBase
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

            var parserBuilder = new EmailCSVParserBuilder(Mapper);
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

            var parserBuilder = new EmailCSVParserBuilder(Mapper);
            parserBuilder.HttpContext = httpContextBuilder.Build();

            var parser = parserBuilder.Build();

            //Act
            parser.ParseAndStore();
            //Assert
        }

        [Test]
        public void ParseAndSave_ShouldReadTheInputStreamFromTheFile()
        {
            //Arrange
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileCount(1);

            var file = MockRepository.GenerateMock<HttpPostedFileBase>();
            file.Expect(x => x.InputStream).Repeat.Once().Return(GetDataStream());

            httpContextBuilder.Replace(file);

            var parserBuilder = new EmailCSVParserBuilder(Mapper);
            parserBuilder.HttpContext = httpContextBuilder.Build();

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();
            //Assert
            file.VerifyAllExpectations();
        }

        [Test]
        public void ParseAndSave_ShouldCallTheStoreMethodOnTheRepositoryWithTheList()
        {
            //Arrange
            var emailsRepository = MockRepository.GenerateMock<IEmailsRepository>();
            emailsRepository.Expect(x => x.Store(Arg<List<Email>>.Is.Anything)).Repeat.Once();

            var parserBuilder = new EmailCSVParserBuilder(Mapper);
            parserBuilder.EmailRepsitory = emailsRepository;

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

    
            //Assert
            emailsRepository.VerifyAllExpectations();

        }

        [Test]
        public void Results_ShouldReturnTheNumberOfFilesProcessed()
        {
            //Arrange
            var parserBuilder = new EmailCSVParserBuilder(Mapper);

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            var results = parser.Results;
            //Assert
            results.NumberOfFilesProcessed.Should().Be(1);

        }

        [Test]
        public void Results_ShouldReturnTheNumberOfEmailsProcessed()
        {
            //Arrange
            var parserBuilder = new EmailCSVParserBuilder(Mapper);

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            var results = parser.Results;
            //Assert
            results.NumberOfEmailProcessed.Should().Be(5);

        }

        [Test]
        public void Results_ShouldReturnTheNamesOFTheFilesThatWereProcessed()
        {
            //Arrange
            var parserBuilder = new EmailCSVParserBuilder(Mapper);

            var parser = parserBuilder.Build();
            //Act
            parser.ParseAndStore();

            var results = parser.Results;
            //Assert
            results.Filenames.Should().HaveCount(1);
            results.Filenames.Should().Contain("emails.csv");

        }


        public static MemoryStream GetDataStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("email,name,country");
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

    public class EmailCSVParserBuilder
    {
        private HttpContextBase httpContext;
        private IEmailsRepository emailRepsitory;
        private IMappingEngine mapper;

        public EmailCSVParserBuilder(IMappingEngine mapper)
        {
            var httpContextBuilder = new HttpContextBaseBuilderForFiles();
            httpContextBuilder.WithFileStream(EmailCSVParserTests.GetDataStream());

            HttpContext = httpContextBuilder.Build();

            EmailRepsitory = MockRepository.GenerateStub<IEmailsRepository>();
            this.mapper = mapper;
        }

        public HttpContextBase HttpContext
        {
            set { httpContext = value; }
        }

        public IEmailsRepository EmailRepsitory
        {
            set { emailRepsitory = value; }
        }

        public EmailCSVParser Build()
        {
            return new EmailCSVParser(httpContext, emailRepsitory, mapper);
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
            fileName = "emails.csv";
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