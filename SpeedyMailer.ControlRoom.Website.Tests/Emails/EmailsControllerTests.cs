using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using MvcContrib.TestHelper;
using SpeedyMailer.ControlRoom.Website.Controllers;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.Builders;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Tests.Emails
{
    [TestFixture]
    public class EmailsControllerTests:AutoMapperAndFixtureBase
    {

        [Test]
        public void UploadList_ShouldCallTheParseMethodOnTheEmailParser()
        {
            //Arrange           
            var controllerBuilder = new EmailsControllerBuilder();

            var emailCSVParser = MockRepository.GenerateMock<IEmailCSVParser>();
            emailCSVParser.Expect(x => x.ParseAndStore()).Repeat.Once();

            controllerBuilder.EmailCSVParser = emailCSVParser;

            var controller = controllerBuilder.Build();

            //Act
            controller.UploadList();
            //Assert
            emailCSVParser.VerifyAllExpectations();

        }

        [Test]
        public void UploadList_ShouldCallTheBuildMethodOnTheBuilder()
        {
            //Arrange
            var builder = MockRepository.GenerateMock<IViewModelBuilderWithBuildParameters<EmailUploadListViewModel, IEmailCSVParser>>();
            builder.ExpectBuild();

            var controllerBuilder = new EmailsControllerBuilder();
            controllerBuilder.EmailUploadViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            controller.UploadList();
            //Assert
            builder.VerifyAllExpectations();
        }

        [Test]
        public void UploadList_ShouldReturnTheRightViewModel()
        {
            //Arrange
            var builder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<EmailUploadListViewModel, IEmailCSVParser>>();
            builder.Stub(x => x.Build(Arg<IEmailCSVParser>.Is.Anything)).Return(new EmailUploadListViewModel());

            var controllerBuilder = new EmailsControllerBuilder();
            controllerBuilder.EmailUploadViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.UploadList();
            //Assert
            viewModel.AssertViewRendered().WithViewData<EmailUploadListViewModel>();
        }


    }

    public class EmailsControllerBuilder:IControllerBuilder<EmailsController>
    {
        public IViewModelBuilderWithBuildParameters<EmailUploadListViewModel, IEmailCSVParser> EmailUploadViewModelBuilder { get; set; }
        public IEmailCSVParser EmailCSVParser { get; set; }

        public EmailsControllerBuilder()
        {
            EmailUploadViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<EmailUploadListViewModel, IEmailCSVParser>>();
            EmailCSVParser = MockRepository.GenerateStub<IEmailCSVParser>();
        }


        public EmailsController Build()
        {
            return new EmailsController(EmailCSVParser,EmailUploadViewModelBuilder);
        }
    }
}