using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Lists;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Web.Tests.Compose
{
    [TestFixture]
    public class ComposeViewModelBuilderTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Build_ShouldReturnTheCollectionOfLists()
        {
            //Arrange

            var listDescriptors = new List<ListDescriptor>();
            listDescriptors.AddMany(() => Fixture.CreateAnonymous<ListDescriptor>(), 5);

            var listsStore = new ListsStore
                                 {
                                     Lists = listDescriptors
                                 };

            var listRepository = MockRepository.GenerateStub<IListRepository>();
            listRepository.Stub(x => x.Lists()).Return(listsStore);

            var builder = new ComposeViewModelBuilder(listRepository, Mapper);
            //Act
            ComposeViewModel viewModel = builder.Build();
            //Assert
            viewModel.AvailableLists.ForEach(x => listDescriptors.First(m => m.Id == x.Id).Should().NotBeNull());
        }
    }
}