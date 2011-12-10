using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Core.Builders
{
    public class UploadListResultsViewModelBuilder : IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>
    {
        private readonly IMappingEngine mapper;
        private readonly IListRepository listRepository;

        public UploadListResultsViewModelBuilder(IMappingEngine mapper, IListRepository listRepository)
        {
            this.mapper = mapper;
            this.listRepository = listRepository;
        }

        public UploadListViewModel Build(IContactsCSVParser parameter)
        {
            var results = parameter.Results;
            var viewModel =  mapper.Map<ContactCSVParserResults, UploadListViewModel>(results);
            viewModel.HasResults = true;

            var listCollection = listRepository.Lists();

            viewModel.Lists = listCollection.Lists;

            return viewModel;
        }
    }
}