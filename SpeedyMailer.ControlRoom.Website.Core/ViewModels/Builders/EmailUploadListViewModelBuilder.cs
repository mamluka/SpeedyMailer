using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Core.ViewModels.Builders
{
    public class EmailUploadListViewModelBuilder:IViewModelBuilderWithBuildParameters<EmailUploadListViewModel,IEmailCSVParser>
    {
        private readonly IMappingEngine mapper;

        public EmailUploadListViewModelBuilder(IMappingEngine mapper)
        {
            this.mapper = mapper;
        }

        public EmailUploadListViewModel Build(IEmailCSVParser parameter)
        {
            var results = parameter.Results;
            return mapper.Map<EmailCSVParserResults, EmailUploadListViewModel>(results);
        }
    }
}