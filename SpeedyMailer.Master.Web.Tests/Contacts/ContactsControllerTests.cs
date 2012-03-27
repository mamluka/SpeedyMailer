using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Lists;
using SpeedyMailer.Master.Web.Tests.Maps;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Master.Web.Tests.Contacts
{
    [TestFixture]
    public class ContactsControllerTests:AutoMapperAndFixtureBase<AutoMapperMaps>
    {

        [Test]
        public void UploadList_ShouldCallTheBuildMethodOnTheBuilder()
        {
            //Arrange
            var builder = MockRepository.GenerateMock<IViewModelBuilder<UploadListViewModel>>();
            builder.ExpectBuild();

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
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
            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
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

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);

            var emailCSVParser = MockRepository.GenerateMock<IContactsCSVParser>();
            emailCSVParser.Expect(
                x =>
                x.AddInitialContactBatchOptions(
                    Arg<InitialContactsBatchOptions>.Matches(m => m.ContainingListId == model.List))).Repeat.Once();

            controllerBuilder.ContactsCSVParser = emailCSVParser;

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

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);

            var emailCSVParser = MockRepository.GenerateMock<IContactsCSVParser>();
            emailCSVParser.Expect(x => x.ParseAndStore()).Repeat.Once();

            controllerBuilder.ContactsCSVParser = emailCSVParser;

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

            var builder = MockRepository.GenerateMock<IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>();
            builder.ExpectBuild();

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
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

            var builder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>();
            builder.Stub(x => x.Build(Arg<IContactsCSVParser>.Is.Anything)).Return(new UploadListViewModel());

            var controllerBuilder = new ContactsMockedComponentBuilder(Mapper);
            controllerBuilder.UploadListResultsViewModelBuilder = builder;

            var controller = controllerBuilder.Build();
            //Act
            var viewModel = controller.Upload(model);
            //Assert
            viewModel.AssertViewRendered().WithViewData<UploadListViewModel>();
        }
    }

    public class ContactsMockedComponentBuilder:IMockedComponentBuilder<ContactsController>
    {
        public IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser> UploadListResultsViewModelBuilder { get; set; }
        public IViewModelBuilder<UploadListViewModel> UploadListViewModelBuilder { get; set; }
        public IContactsCSVParser ContactsCSVParser { get; set; }
        public IListRepository ListRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public ContactsMockedComponentBuilder(IMappingEngine mapper)
        {
            UploadListResultsViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>();
            ContactsCSVParser = MockRepository.GenerateStub<IContactsCSVParser>();
            ListRepository = MockRepository.GenerateStub<IListRepository>();
            UploadListViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilder<UploadListViewModel>>();
            Mapper = mapper;
        }


        public ContactsController Build()
        {
            return new ContactsController(ContactsCSVParser, UploadListResultsViewModelBuilder,UploadListViewModelBuilder, Mapper);
        }
    }
}