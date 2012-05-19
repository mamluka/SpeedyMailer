using AutoMapper;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Lists;
using SpeedyMailer.Core.Utilities.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Master.Web.UI.Controllers;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Web.Tests.Unit.Contacts
{
    public class ContactsMockedComponentBuilder : IMockedComponentBuilder<ContactsController>
    {
        public ContactsMockedComponentBuilder(IMappingEngine mapper)
        {
            UploadListResultsViewModelBuilder =
                MockRepository.GenerateStub
                    <IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>();
            ContactsCSVParser = MockRepository.GenerateStub<IContactsCSVParser>();
            ListRepository = MockRepository.GenerateStub<IListRepository>();
            UploadListViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilder<UploadListViewModel>>();
            Mapper = mapper;
        }

        public IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>
            UploadListResultsViewModelBuilder { get; set; }

        public IViewModelBuilder<UploadListViewModel> UploadListViewModelBuilder { get; set; }
        public IContactsCSVParser ContactsCSVParser { get; set; }
        public IListRepository ListRepository { get; set; }
        public IMappingEngine Mapper { get; set; }


        public ContactsController Build()
        {
            return new ContactsController(ContactsCSVParser, UploadListResultsViewModelBuilder,
                                          UploadListViewModelBuilder, Mapper);
        }

    }
}