using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Core.Lists;
using SpeedyMailer.Tests.Core;
using System.Linq;

namespace SpeedyMailer.ControlRoom.Website.Tests.Compose
{
    [TestFixture]
    public class ComposeViewModelBuilderTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Build_ShouldReturnTheCollectionOfLists()
        {
            //Arrange

            var listDescriptors = new List<ListDescriptor>();
            listDescriptors.AddMany(() => Fixture.CreateAnonymous<ListDescriptor>(),5);

            var listsStore = new ListsStore()
                                     {
                                         Lists = listDescriptors
                                     };

            var listRepository = MockRepository.GenerateStub<IListRepository>();
            listRepository.Stub(x => x.Lists()).Return(listsStore);

            var builder = new ComposeViewModelBuilder(listRepository,Mapper);
            //Act
            var viewModel = builder.Build();
            //Assert
            viewModel.AvalibaleLists.ForEach(x=> listDescriptors.First(m=> m.Id == x.Id).Should().NotBeNull());

        }

    }
}