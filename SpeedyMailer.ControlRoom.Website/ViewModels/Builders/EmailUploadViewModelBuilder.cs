using System;
using SpeedyMailer.ControlRoom.Website.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.ViewModels.Builders
{
    public class EmailUploadViewModelBuilder:IViewModelBuilderWithBuildParameters<EmailUploadViewModel,IEmailCSVParser>
    {
        public EmailUploadViewModel Build(IEmailCSVParser parameter)
        {
            return new EmailUploadViewModel();
        }
    }
}