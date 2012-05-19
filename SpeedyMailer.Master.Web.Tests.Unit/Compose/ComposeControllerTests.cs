using System;
using System.Web.Mvc;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Master.Web.UI.Controllers;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Web.Tests.Unit.Compose
{
    internal class ComposeControllerTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Index_ShouldRenderTheRightViewModel()
        {
            //Arrange
            var controllerBuilder = new ComposeControllerBuilder(Mapper);

            ComposerController controller = controllerBuilder.Build();
            //Act
            ActionResult viewModel = controller.Index();
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

            ComposerController controller = controllerBuilder.Build();
            //Act
            ActionResult viewModel = controller.Index();
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

            ComposerController controller = controllerBuilder.Build();
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
            Email email = Mapper.Map<ComposeModel, Email>(model);

            var controllerBuilder = new ComposeControllerBuilder(Mapper);
            var emailRepository = MockRepository.GenerateMock<IEmailRepository>();
            emailRepository.Expect(x => x.Store(Arg<Email>.Matches(m => m.Id == email.Id))).Repeat.Once();

            controllerBuilder.EmailRepository = emailRepository;

            ComposerController controller = controllerBuilder.Build();
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

            ComposerController controller = controllerBuilder.Build();
            //Act
            controller.Index(model);
            //Assert
            emailPool.VerifyAllExpectations();
        }


        private bool CompareTwoStringLists(ComposeModel model, Email m)
        {
            return String.Join(",", m.Lists) == String.Join(",", model.ToLists);
        }
    }
}