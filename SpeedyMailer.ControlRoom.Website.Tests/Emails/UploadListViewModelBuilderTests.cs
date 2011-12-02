using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Core.Lists;
using SpeedyMailer.Tests.Core;


namespace SpeedyMailer.ControlRoom.Website.Tests.Emails
{
    [TestFixture]
    public class UploadListViewModelBuilderTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Build_ShouldLoadTheListCollectionFromTheSystemUsingTheRepository()
        {
            //Arrange
            var listRepository = MockRepository.GenerateMock<IListRepository>();

            listRepository.Expect(x => x.Lists()).Repeat.Once();

            var builder = new UploadListViewModelBuilder(listRepository);
            //Act
            builder.Build();
            //Assert
            listRepository.VerifyAllExpectations();
        }

        [Test]
        public void Build_ShouldReturnTheListCollectionOfTheSystem()
        {
            //Arrange
            var allLists = new List<ListDescriptor>();
            allLists.AddMany(() => Fixture.CreateAnonymous<ListDescriptor>(),10);

            var listRepository = MockRepository.GenerateStub<IListRepository>();

            listRepository.Stub(x=> x.Lists()).Return(allLists);

            var builder = new UploadListViewModelBuilder(listRepository);
            //Act
            var viewModel = builder.Build();
            //Assert
            viewModel.Lists.Should().BeEquivalentTo(allLists);
        }
    }

    public class UploadListViewModelBuilder:IViewModelBuilder<UploadListViewModel>
    {
        private readonly IListRepository listRepository;

        public UploadListViewModelBuilder(IListRepository listRepository)
        {
            this.listRepository = listRepository;
        }

        public UploadListViewModel Build()
        {
            var allLists = listRepository.Lists();

            return new UploadListViewModel()
                       {
                           Lists = allLists
                       };
        }
    }
}