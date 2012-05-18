using Nancy.Bootstrappers.Ninject;
using Ninject;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.DataAccess.Drone;
using SpeedyMailer.Core.DataAccess.Fragments;
using SpeedyMailer.Master.Service.Emails;
using SpeedyMailer.Master.Service.MailDrones;

namespace SpeedyMailer.Master.Service.Tests.Pool
{
    public class MyNinjectBootstrapperWithMockedObjects : NinjectNancyBootstrapper
    {
        public MyNinjectBootstrapperWithMockedObjects()
        {
            MailDroneService = MockRepository.GenerateStub<IMailDroneService>();
            MailDroneRepository = MockRepository.GenerateStub<IMailDroneRepository>();
            MailOporations = MockRepository.GenerateStub<IPoolMailOporations>();
            FragmentRepository = MockRepository.GenerateStub<IFragmentRepository>();

            FragmentRepository.Stub(x => x.PopFragment()).Return(new EmailFragment());
        }

        public IMailDroneRepository MailDroneRepository { get; set; }
        public IMailDroneService MailDroneService { get; set; }
        public IPoolMailOporations MailOporations { get; set; }
        public IFragmentRepository FragmentRepository { get; set; }

        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            existingContainer.Bind<IMailDroneRepository>().ToConstant(MailDroneRepository);
            existingContainer.Bind<IMailDroneService>().ToConstant(MailDroneService);
            existingContainer.Bind<IPoolMailOporations>().ToConstant(MailOporations);
            existingContainer.Bind<IFragmentRepository>().ToConstant(FragmentRepository);
        }
    }
}