using System;
using System.Linq;
using System.Text;
using AutoMapper;
using NUnit;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.ControlRoom.Website.Controllers;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Tests.Core;
using MvcContrib.TestHelper;
using Ploeh.AutoFixture;

namespace SpeedyMailer.ControlRoom.Website.Tests.Compose
{
    internal class ComposeControllerTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Index_ShouldRenderTheRightViewModel()
        {
            //Arrange
            var controllerBuilder = new ComposeControllerBuilder(Mapper);

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.Index();
            //Assert
            viewModel.AssertViewRendered().WithViewData<ComposeViewModel>();
        }

        [Test]
        public void Index_ShouldBuildTheViewModelUsingTheBuilder()
        {
            //Arrange
            var controllerBuilder = new ComposeControllerBuilder(Mapper);

            var builder = MockRepository.GenerateMock<IViewModelBuilder<ComposeViewModel>>();
            builder.ExpectBuild();

            controllerBuilder.IndexViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.Index();
            //Assert
          builder.VerifyAllExpectations();
        }

        [Test]
        public void PostIndex_ShouldAddTheComposedEmailToTheEmailPool()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<ComposeModel>();

            var controllerBuilder = new ComposeControllerBuilder(Mapper);
            var emailPool = MockRepository.GenerateMock<IEmailPool>();
            emailPool.Expect(x => x.AddEmail(Arg<Email>.Is.Anything)).Repeat.Once();

            controllerBuilder.EmailPool = emailPool;

            var controller = controllerBuilder.Build();
            //Act
            controller.Index(model);
            //Assert
            emailPool.VerifyAllExpectations();
        }

        [Test]
        public void PostIndex_ShouldPassTheEmailParameters()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<ComposeModel>();

            var controllerBuilder = new ComposeControllerBuilder(Mapper);
            var emailPool = MockRepository.GenerateMock<IEmailPool>();
            emailPool.Expect(x => x.AddEmail(Arg<Email>.Matches(
                m => m.Body == model.Body &&
                     CompareTwoStringLists(model, m)
                                                 ))).Repeat.Once();

            controllerBuilder.EmailPool = emailPool;

            var controller = controllerBuilder.Build();
            //Act
            controller.Index(model);
            //Assert
            emailPool.VerifyAllExpectations();
        }

       

        private bool CompareTwoStringLists(ComposeModel model, Email m)
        {
            return String.Join(",", m.ToLists) == String.Join(",", model.ToLists);
        }


    }

    public interface IEmailRepository
    {
    }

    public class ComposeControllerBuilder:IMockedComponentBuilder<ComposeController>
    {
        public IViewModelBuilder<ComposeViewModel> IndexViewModelBuilder { get; set; }
        public IEmailPool EmailPool { get; set; }
        public IMappingEngine Mapper { get; set; }

        public ComposeControllerBuilder(IMappingEngine mapper)
        {
            IndexViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilder<ComposeViewModel>>();

            IndexViewModelBuilder.Stub(x => x.Build()).Return(new ComposeViewModel());

            EmailPool = MockRepository.GenerateStub<IEmailPool>();

            Mapper = mapper;
        }
        public ComposeController Build()
        {
            return new ComposeController(IndexViewModelBuilder,EmailPool,Mapper);
        }
    }
}
