using System.Collections.Generic;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Master.Web.UI.Communication;
using SpeedyMailer.Master.Web.UI.Jobs;
using SpeedyMailer.Master.Web.UI.Mail;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Mail
{
    internal class MockedRetrieveFragmentJobBuilder : IMockedComponentBuilder<RetrieveFragmentJob>
    {
        public MockedRetrieveFragmentJobBuilder()
        {
            DroneCommunicationService = MockRepository.GenerateStub<IDroneCommunicationService>();

            MailOporations = MockRepository.GenerateStub<IDroneMailOporations>();

            MailSender = MockRepository.GenerateStub<IMailSender>();
        }

        public IDroneCommunicationService DroneCommunicationService { get; set; }
        public IDroneMailOporations MailOporations { get; set; }
        public IMailSender MailSender { get; set; }


        public RetrieveFragmentJob Build()
        {
            return new RetrieveFragmentJob(DroneCommunicationService, MailOporations, MailSender);
        }


        public MockedRetrieveFragmentJobBuilder WithFragmentResponse(FragmentResponse fragmentResponse = null)
        {
            if (fragmentResponse == null)
            {
                fragmentResponse = new FragmentResponse
                                       {
                                           DroneSideOporations = new List<DroneSideOporationBase>(),
                                           EmailFragment = new EmailFragment()
                                       };
            }


            DroneCommunicationService.Stub(x => x.RetrieveFragment()).Return(fragmentResponse);

            return this;
        }
    }
}