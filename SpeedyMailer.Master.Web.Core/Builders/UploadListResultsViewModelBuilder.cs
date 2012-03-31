using AutoMapper;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.DataAccess.Lists;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.Core.Builders
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