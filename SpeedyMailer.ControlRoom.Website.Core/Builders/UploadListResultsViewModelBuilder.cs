using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Core.Builders
{
    public class UploadListResultsViewModelBuilder : IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser>
    {
        private readonly IMappingEngine mapper;

        public UploadListResultsViewModelBuilder(IMappingEngine mapper)
        {
            this.mapper = mapper;
        }

        public UploadListViewModel Build(IEmailCSVParser parameter)
        {
            var results = parameter.Results;
            return mapper.Map<EmailCSVParserResults, UploadListViewModel>(results);
        }
    }
}