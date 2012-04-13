using AutoMapper;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Master.Web.UI.Controllers;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Web.Tests.Compose
{
    public class ComposeControllerBuilder : IMockedComponentBuilder<ComposerController>
    {
        public ComposeControllerBuilder(IMappingEngine mapper)
        {
            IndexViewModelBuilder = MockRepository.GenerateStub<IViewModelBuilder<ComposeViewModel>>();

            IndexViewModelBuilder.Stub(x => x.Build()).Return(new ComposeViewModel());

            EmailPoolService = MockRepository.GenerateStub<IEmailPoolService>();

            EmailRepository = MockRepository.GenerateStub<IEmailRepository>();

            Mapper = mapper;
        }

        public IViewModelBuilder<ComposeViewModel> IndexViewModelBuilder { get; set; }
        public IEmailPoolService EmailPoolService { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IEmailRepository EmailRepository { get; set; }


        public ComposerController Build()
        {
            return new ComposerController(IndexViewModelBuilder, EmailRepository, EmailPoolService, Mapper);
        }

    }
}