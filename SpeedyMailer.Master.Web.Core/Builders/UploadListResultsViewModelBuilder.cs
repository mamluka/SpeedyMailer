using AutoMapper;
using SpeedyMailer.Core.DataAccess.Lists;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Utilties.Domain.Contacts;

namespace SpeedyMailer.Master.Web.Core.Builders
{
    public class UploadListResultsViewModelBuilder :
        IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>
    {
        private readonly IListRepository listRepository;
        private readonly IMappingEngine mapper;

        public UploadListResultsViewModelBuilder(IMappingEngine mapper, IListRepository listRepository)
        {
            this.mapper = mapper;
            this.listRepository = listRepository;
        }


        public UploadListViewModel Build(IContactsCSVParser parameter)
        {
            ContactCSVParserResults results = parameter.Results;
            UploadListViewModel viewModel = mapper.Map<ContactCSVParserResults, UploadListViewModel>(results);
            viewModel.HasResults = true;

            ListsStore listCollection = listRepository.Lists();

            viewModel.Lists = listCollection.Lists;

            return viewModel;
        }

    }
}