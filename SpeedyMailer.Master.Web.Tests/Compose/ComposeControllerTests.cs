using System;
using AutoMapper;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Domain.Model.Emails;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Master.Web.Tests.Maps;
using SpeedyMailer.Master.Web.UI.Controllers;
using SpeedyMailer.Tests.Core;
using Ploeh.AutoFixture;

namespace SpeedyMailer.Master.Web.Tests.Compose
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
            var emailPool = MockRepository.GenerateMock<IEmailPoolService>();
            emailPool.Expect(x => x.AddEmail(Arg<Email>.Is.Anything)).Repeat.Once();

            controllerBuilder.EmailPoolService = emailPool;

            var controller = controllerBuilder.Build();
            //Act
            controller.Index(model);
            //Assert
            emailPool.VerifyAllExpectations();
        }

        [Test]
        public void PostIndex_ShouldAddEmailToTheEmailRepository()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<ComposeModel>();
            var email = Mapper.Map<ComposeModel, Email>(model);

            var controllerBuilder = new ComposeControllerBuilder(Mapper);
            var emailRepository = MockRepository.GenerateMock<IEmailRepository>();
            emailRepository.Expect(x => x.Store(Arg<Email>.Matches(m=> m.Id == email.Id))).Repeat.Once();

            controllerBuilder.EmailRepository = emailRepository;

            var controller = controllerBuilder.Build();
            //Act
            controller.Index(model);
            //Assert
            emailRepository.VerifyAllExpectations();
        }

        [Test]
        public void PostIndex_ShouldPassTheEmailParameters()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<ComposeModel>();

            var controllerBuilder = new ComposeControllerBuilder(Mapper);
            var emailPool = MockRepository.GenerateMock<IEmailPoolService>();
            emailPool.Expect(x => x.AddEmail(Arg<Email>.Matches(
                m => m.Body == model.Body &&
                     CompareTwoStringLists(model, m)
                                                 ))).Repeat.Once();

            controllerBuilder.EmailPoolService = emailPool;

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

    public class ComposeControllerBuilder:IMockedComponentBuilder<ComposeController>
    {
        public IViewModelBuilder<ComposeViewModel> IndexViewModelBuilder { get; set; }
        public IEmailPoolService EmailPoolService { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IEmailRepository EmailRepository { get; set; }

        public ComposeControllerBuilder(IMappingEngine mapper)
        {
            IndexViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilder<ComposeViewModel>>();

            IndexViewModelBuilder.Stub(x => x.Build()).Return(new ComposeViewModel());

            EmailPoolService = MockRepository.GenerateStub<IEmailPoolService>();

            EmailRepository = MockRepository.GenerateStub<IEmailRepository>();

            Mapper = mapper;
        }
        public ComposeController Build()
        {
            return new ComposeController(IndexViewModelBuilder,EmailRepository,EmailPoolService,Mapper);
        }
    }
}
