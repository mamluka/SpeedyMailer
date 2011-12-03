using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Core.Emails;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Lists;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.ControlRoom.Website.Tests.Emails
{
    class UploadListResultsViewModelBuilderTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Build_ShouldReadTheResultsFromTheCSVParser()
        {
            //Arrange
            var csvHelper = MockRepository.GenerateMock<IEmailCSVParser>();

            csvHelper.Expect(x => x.Results).Repeat.Once();

            var componentBuilder = new UploadListResultsViewModelBuilderMockedComponentBuilder(Mapper);
            var builder = componentBuilder.Build();

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

            var csvParserResults =
                Fixture.Build<EmailCSVParserResults>().With(x => x.Filenames, fileList).CreateAnonymous();

            csvHelper.Stub(x => x.Results).Return(csvParserResults);

            var componentBuilder = new UploadListResultsViewModelBuilderMockedComponentBuilder(Mapper);
            var builder = componentBuilder.Build();

            //Act
            var viewModel = builder.Build(csvHelper);
            //Assert

            viewModel.NumberOfEmailProcessed.Should().Be(csvParserResults.NumberOfEmailProcessed.ToString());
            viewModel.NumberOfFilesProcessed.Should().Be(csvParserResults.NumberOfFilesProcessed.ToString());
            viewModel.Filenames.Should().BeEquivalentTo(fileList);
        }

        [Test]
        public void Build_ShouldSayToTheViewThatThereAreResults()
        {
            //Arrange
            var csvHelper = MockRepository.GenerateMock<IEmailCSVParser>();

            csvHelper.Expect(x => x.Results).Repeat.Once();

            var componentBuilder = new UploadListResultsViewModelBuilderMockedComponentBuilder(Mapper);
            var builder = componentBuilder.Build();

            //Act
            var viewModel = builder.Build(csvHelper);
            //Assert

            viewModel.HasResults.Should().BeTrue();

        }

        [Test]
        public void Buils_ShouldLoadTheListCollection()
        {
            //Arrange
            var csvHelper = MockRepository.GenerateStub<IEmailCSVParser>();

            csvHelper.Stub(x => x.Results).Repeat.Once();

            var listRepository = MockRepository.GenerateMock<IListRepository>();
            listRepository.Expect(x => x.Lists()).Repeat.Once();

            var builder = new UploadListResultsViewModelBuilder(Mapper, listRepository);

            //Act
            builder.Build(csvHelper);
            //Assert

            listRepository.VerifyAllExpectations();

        }

        public class UploadListResultsViewModelBuilderMockedComponentBuilder : IMockedComponentBuilder<UploadListResultsViewModelBuilder>
        {
            public IMappingEngine Mapper { get; set; }
            public IListRepository ListRepository { get; set; }

            public UploadListResultsViewModelBuilderMockedComponentBuilder(IMappingEngine mapper)
            {
                Mapper = mapper;
                ListRepository = MockRepository.GenerateStub<IListRepository>();

                ListRepository.Stub(x => x.Lists());

            }

            public UploadListResultsViewModelBuilder Build()
            {
                return new UploadListResultsViewModelBuilder(Mapper,ListRepository);
            }
        }

    }


}
