using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using MvcContrib.TestHelper;
using SpeedyMailer.ControlRoom.Website.Controllers;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Core.Emails;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Lists;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.ControlRoom.Website.Tests.Emails
{
    [TestFixture]
    public class EmailsControllerTests:AutoMapperAndFixtureBase<AutoMapperMaps>
    {

        [Test]
        public void UploadList_ShouldCallTheBuildMethodOnTheBuilder()
        {
            //Arrange
            var builder = MockRepository.GenerateMock<IViewModelBuilder<UploadListViewModel>>();
            builder.ExpectBuild();

            var controllerBuilder = new EmailsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            controller.Upload();
            //Assert
            builder.VerifyAllExpectations();

        }

        [Test]
        public void UploadList_ShouldReturnTheRightViewModel()
        {
            //Arrange
            var controllerBuilder = new EmailsMockedComponentBuilder(Mapper);
            var builder = MockRepository.GenerateStub<IViewModelBuilder<UploadListViewModel>>();
            builder.Stub(x => x.Build()).Return(new UploadListViewModel());

            controllerBuilder.UploadListViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.Upload();
            //Assert
            viewModel.AssertViewRendered().WithViewData<UploadListViewModel>();

        }

        [Test]
        public void UploadListPost_ShouldAddTheInitialParametersToTheCSVBuilderContainingTheCategoryIDFromTheModel()
        {
            //Arrange       

            var model = Fixture.CreateAnonymous<UploadListModel>();

            var controllerBuilder = new EmailsMockedComponentBuilder(Mapper);

            var emailCSVParser = MockRepository.GenerateMock<IEmailCSVParser>();
            emailCSVParser.Expect(
                x =>
                x.AddInitialEmailBatchOptions(
                    Arg<InitialEmailBatchOptions>.Matches(m => m.ContainingListId == model.List))).Repeat.Once();

            controllerBuilder.EmailCSVParser = emailCSVParser;

            var controller = controllerBuilder.Build();

            //Act
            controller.Upload(model);
            //Assert
            emailCSVParser.VerifyAllExpectations();

        }


        [Test]
        public void UploadListPost_ShouldCallTheParseMethodOnTheEmailParser()
        {
            //Arrange       

            var model = Fixture.CreateAnonymous<UploadListModel>();

            var controllerBuilder = new EmailsMockedComponentBuilder(Mapper);

            var emailCSVParser = MockRepository.GenerateMock<IEmailCSVParser>();
            emailCSVParser.Expect(x => x.ParseAndStore()).Repeat.Once();

            controllerBuilder.EmailCSVParser = emailCSVParser;

            var controller = controllerBuilder.Build();

            //Act
            controller.Upload(model);
            //Assert
            emailCSVParser.VerifyAllExpectations();

        }

        [Test]
        public void UploadListPost_ShouldCallTheBuildMethodOnTheBuilder()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<UploadListModel>();

            var builder = MockRepository.GenerateMock<IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser>>();
            builder.ExpectBuild();

            var controllerBuilder = new EmailsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListResultsViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            controller.Upload(model);
            //Assert
            builder.VerifyAllExpectations();
        }

        [Test]
        public void UploadListPost_ShouldReturnTheRightViewModel()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<UploadListModel>();

            var builder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser>>();
            builder.Stub(x => x.Build(Arg<IEmailCSVParser>.Is.Anything)).Return(new UploadListViewModel());

            var controllerBuilder = new EmailsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListResultsViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.Upload(model);
            //Assert
            viewModel.AssertViewRendered().WithViewData<UploadListViewModel>();
        }
    }

    public class EmailsMockedComponentBuilder:IMockedComponentBuilder<EmailsController>
    {
        public IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser> UploadListResultsViewModelBuilder { get; set; }
        public IViewModelBuilder<UploadListViewModel> UploadListViewModelBuilder { get; set; }
        public IEmailCSVParser EmailCSVParser { get; set; }
        public IListRepository ListRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public EmailsMockedComponentBuilder(IMappingEngine mapper)
        {
            UploadListResultsViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser>>();
            EmailCSVParser = MockRepository.GenerateStub<IEmailCSVParser>();
            ListRepository = MockRepository.GenerateStub<IListRepository>();
            UploadListViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilder<UploadListViewModel>>();
            Mapper = mapper;
        }


        public EmailsController Build()
        {
            return new EmailsController(EmailCSVParser, UploadListResultsViewModelBuilder,UploadListViewModelBuilder, Mapper);
        }
    }
}