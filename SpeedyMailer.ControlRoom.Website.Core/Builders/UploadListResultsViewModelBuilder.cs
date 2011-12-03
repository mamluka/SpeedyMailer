using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Core.Builders
{
    public class UploadListResultsViewModelBuilder : IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser>
    {
        private readonly IMappingEngine mapper;
        private readonly IListRepository listRepository;

        public UploadListResultsViewModelBuilder(IMappingEngine mapper, IListRepository listRepository)
        {
            this.mapper = mapper;
            this.listRepository = listRepository;
        }

        public UploadListViewModel Build(IEmailCSVParser parameter)
        {
            var results = parameter.Results;
            var viewModel =  mapper.Map<EmailCSVParserResults, UploadListViewModel>(results);
            viewModel.HasResults = true;

            var listCollection = listRepository.Lists();

            viewModel.Lists = listCollection.Lists;

            return viewModel;
        }
    }
}