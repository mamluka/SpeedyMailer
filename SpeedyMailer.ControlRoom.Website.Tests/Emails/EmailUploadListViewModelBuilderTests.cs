using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.Builders;
using SpeedyMailer.Core.Emails;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Tests;

namespace SpeedyMailer.ControlRoom.Website.Tests.Emails
{
    class EmailUploadListViewModelBuilderTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Build_ShouldReadTheResultsFromTheCSVParser()
        {
            //Arrange
            var csvHelper = MockRepository.GenerateMock<IEmailCSVParser>();

            csvHelper.Expect(x => x.Results).Repeat.Once();

            var builder = new EmailUploadListViewModelBuilder(Mapper);

            //Act
            builder.Build(csvHelper);
            //Assert

            csvHelper.VerifyAllExpectations();
        }

        [Test]
        public void Build_ShouldReturnTheRightReultsBasedOnParsedData()
        {
            //Arrange
            var csvHelper = MockRepository.GenerateStub<IEmailCSVParser>();

            var fileList = new List<string> {"csv1.txt,csv2.txt"};

            var csvParserResults = Fixture.Build<EmailCSVParserResults>().With(x=> x.Filenames,fileList).CreateAnonymous();

            csvHelper.Stub(x => x.Results).Return(csvParserResults);

            var builder = new EmailUploadListViewModelBuilder(Mapper);

            //Act
            var viewModel =  builder.Build(csvHelper);
            //Assert

            viewModel.NumberOfEmailProcessed.Should().Be(csvParserResults.NumberOfEmailProcessed.ToString());
            viewModel.NumberOfFilesProcessed.Should().Be(csvParserResults.NumberOfFilesProcessed.ToString());
            viewModel.Filenames.Should().BeEquivalentTo(fileList);
        }

    }
}
