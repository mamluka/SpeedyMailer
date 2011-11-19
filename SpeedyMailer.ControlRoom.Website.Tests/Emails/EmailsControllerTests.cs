using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using MvcContrib.TestHelper;
using SpeedyMailer.ControlRoom.Website.Controllers;
using SpeedyMailer.ControlRoom.Website.ViewModels.Builders;
using SpeedyMailer.ControlRoom.Website.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Tests.Emails
{
    [TestFixture]
    public class EmailsControllerTests:AutoMapperAndFixtureBase
    {

        [Test]
        public void Upload_ShouldCallTheParseMethodOnTheEmailParser()
        {
            //Arrange           
            var controllerBuilder = new EmailsControllerBuilder();

            var emailCSVParser = MockRepository.GenerateMock<IEmailCSVParser>();
            emailCSVParser.Expect(x => x.ParseAndStore()).Repeat.Once();

            controllerBuilder.EmailCSVParser = emailCSVParser;

            var controller = controllerBuilder.Build();

            //Act
            controller.Upload();
            //Assert
            emailCSVParser.VerifyAllExpectations();

        }

        [Test]
        public void Upload_ShouldCallTheBuildMethodOnTheBuilder()
        {
            //Arrange
            var builder = MockRepository.GenerateMock<IViewModelBuilderWithBuildParameters<EmailUploadViewModel, IEmailCSVParser>>();
            builder.ExpectBuild();

            var controllerBuilder = new EmailsControllerBuilder();
            controllerBuilder.EmailUploadViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            controller.Upload();
            //Assert
            builder.VerifyAllExpectations();
        }

        [Test]
        public void Upload_ShouldReturnTheRightViewModel()
        {
            //Arrange
            var builder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<EmailUploadViewModel, IEmailCSVParser>>();
            builder.Stub(x => x.Build(Arg<IEmailCSVParser>.Is.Anything)).Return(new EmailUploadViewModel());

            var controllerBuilder = new EmailsControllerBuilder();
            controllerBuilder.EmailUploadViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.Upload();
            //Assert
            viewModel.AssertViewRendered().WithViewData<EmailUploadViewModel>();
        }


    }

    public class EmailsControllerBuilder:IControllerBuilder<EmailsController>
    {
        public IViewModelBuilderWithBuildParameters<EmailUploadViewModel, IEmailCSVParser> EmailUploadViewModelBuilder { get; set; }
        public IEmailCSVParser EmailCSVParser { get; set; }

        public EmailsControllerBuilder()
        {
            EmailUploadViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<EmailUploadViewModel, IEmailCSVParser>>();
            EmailCSVParser = MockRepository.GenerateStub<IEmailCSVParser>();
        }


        public EmailsController Build()
        {
            return new EmailsController(EmailCSVParser,EmailUploadViewModelBuilder);
        }
    }
}