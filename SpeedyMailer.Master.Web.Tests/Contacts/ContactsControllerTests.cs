using System.Web.Mvc;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Core.Utilities.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Master.Web.UI.Controllers;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Web.Tests.Contacts
{
    [TestFixture]
    public class ContactsControllerTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void UploadListPost_ShouldAddTheInitialParametersToTheCSVBuilderContainingTheCategoryIDFromTheModel()
        {
            //Arrange       

            var model = Fixture.CreateAnonymous<UploadListModel>();

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);

            var emailCSVParser = MockRepository.GenerateMock<IContactsCSVParser>();
            emailCSVParser.Expect(
                x =>
                x.AddInitialContactBatchOptions(
                    Arg<InitialContactsBatchOptions>.Matches(m => m.ContainingListId == model.List))).Repeat.Once();

            controllerBuilder.ContactsCSVParser = emailCSVParser;

            ContactsController controller = controllerBuilder.Build();

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

            var builder =
                MockRepository.GenerateMock
                    <IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>();
            builder.ExpectBuild();

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListResultsViewModelBuilder = builder;

            ContactsController controller = controllerBuilder.Build();
            //Act
            controller.Upload(model);
            //Assert
            builder.VerifyAllExpectations();
        }

        [Test]
        public void UploadListPost_ShouldCallTheParseMethodOnTheEmailParser()
        {
            //Arrange       

            var model = Fixture.CreateAnonymous<UploadListModel>();

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);

            var emailCSVParser = MockRepository.GenerateMock<IContactsCSVParser>();
            emailCSVParser.Expect(x => x.ParseAndStore()).Repeat.Once();

            controllerBuilder.ContactsCSVParser = emailCSVParser;

            ContactsController controller = controllerBuilder.Build();

            //Act
            controller.Upload(model);
            //Assert
            emailCSVParser.VerifyAllExpectations();
        }

        [Test]
        public void UploadListPost_ShouldReturnTheRightViewModel()
        {
            //Arrange
            var model = Fixture.CreateAnonymous<UploadListModel>();

            var builder =
                MockRepository.GenerateStub
                    <IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>();
            builder.Stub(x => x.Build(Arg<IContactsCSVParser>.Is.Anything)).Return(new UploadListViewModel());

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListResultsViewModelBuilder = builder;

            ContactsController controller = controllerBuilder.Build();
            //Act
            ActionResult viewModel = controller.Upload(model);
            //Assert
            viewModel.AssertViewRendered().WithViewData<UploadListViewModel>();
        }

        [Test]
        public void UploadList_ShouldCallTheBuildMethodOnTheBuilder()
        {
            //Arrange
            var builder = MockRepository.GenerateMock<IViewModelBuilder<UploadListViewModel>>();
            builder.ExpectBuild();

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListViewModelBuilder = builder;

            ContactsController controller = controllerBuilder.Build();
            //Act
            controller.Upload();
            //Assert
            builder.VerifyAllExpectations();
        }

        [Test]
        public void UploadList_ShouldReturnTheRightViewModel()
        {
            //Arrange
            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
            var builder = MockRepository.GenerateStub<IViewModelBuilder<UploadListViewModel>>();
            builder.Stub(x => x.Build()).Return(new UploadListViewModel());

            controllerBuilder.UploadListViewModelBuilder = builder;

            ContactsController controller = controllerBuilder.Build();
            //Act
            ActionResult viewModel = controller.Upload();
            //Assert
            viewModel.AssertViewRendered().WithViewData<UploadListViewModel>();
        }
    }
}