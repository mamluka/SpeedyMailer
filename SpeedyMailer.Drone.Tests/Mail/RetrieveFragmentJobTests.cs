using System.Collections.Generic;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Quartz;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Drone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;

namespace SpeedyMailer.Drone.Tests.Mail
{
    [TestFixture]
    public class RetrieveFragmentJobTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Execute_ShouldGetTheMailFragmentUsingTheDroneCommunication()
        {
            //Arrange
            var fakeJobContext = FakeContext();

            var fragmentReponse = Fixture.CreateAnonymous<FragmentResponse>();

            var droneCom = MockRepository.GenerateMock<IDroneCommunicationService>();
            droneCom.Expect(x => x.RetrieveFragment()).Repeat.Once().Return(fragmentReponse);

            var builder = new MockedRetrieveFragmentJobBuilder();
            builder.DroneCommunicationService = droneCom;
            var job = builder.Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            droneCom.VerifyAllExpectations();

        }

        [Test]
        public void Execute_ShouldExecuteTheFragmentOporations()
        {
            //Arrange
            var fakeJobContext = FakeContext();

            var fragmentReponse = Fixture.CreateAnonymous<FragmentResponse>();
            fragmentReponse.DroneSideOporations = new List<DroneSideOporationBase>()
                                                      {
                                                          new DroneSideOporationBase(),
                                                          new DroneSideOporationBase(),
                                                          new DroneSideOporationBase()
                                                      };
            fragmentReponse.EmailFragment = new EmailFragment();

            var mailOporation = MockRepository.GenerateMock<IDroneMailOporations>();
            mailOporation.Expect(x => x.Preform(Arg<DroneSideOporationBase>.Is.Anything)).Repeat.Times(3);

            var builder = new MockedRetrieveFragmentJobBuilder();
            builder.MailOporations = mailOporation;

            var job = builder.WithFragmentResponse(fragmentReponse).Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            mailOporation.VerifyAllExpectations();

        }

        [Test]
        public void Execute_ShouldSendTheEmailFragment()
        {
            //Arrange
            var fakeJobContext = FakeContext();

            var fragmentReponse = Fixture.CreateAnonymous<FragmentResponse>();

            var mailSender = MockRepository.GenerateMock<IMailSender>();
            mailSender.Expect(x => x.ProcessFragment(Arg<EmailFragment>.Is.Equal(fragmentReponse.EmailFragment))).Repeat.Once();


            var builder = new MockedRetrieveFragmentJobBuilder();
            builder.MailSender = mailSender;

            var job = builder.WithFragmentResponse(fragmentReponse).Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            mailSender.VerifyAllExpectations();

        }

        [Test]
        public void Execute_ShouldContinueTheCurrentJobIfNoPutASleepOporationWasPresent()
        {
            //Arrange
            var fakeScheduler = MockRepository.GenerateMock<IScheduler>();
            fakeScheduler.Expect(
                x => x.RescheduleJob(Arg<TriggerKey>.Matches(m => m.Name == "MailTrigger"), Arg<ITrigger>.Is.Anything)).
                Return(null).
                Repeat.Once();

            var fakeJobContext = MockRepository.GenerateStub<IJobExecutionContext>();
            fakeJobContext.Stub(x => x.Scheduler).Return(fakeScheduler);



            var builder = new MockedRetrieveFragmentJobBuilder();
            var job = builder.WithFragmentResponse().Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            fakeScheduler.VerifyAllExpectations();
        }

        private IJobExecutionContext FakeContext()
        {
            var fakeScheduler = MockRepository.GenerateStub<IScheduler>();

            var fakeJobContext = MockRepository.GenerateStub<IJobExecutionContext>();
            fakeJobContext.Stub(x => x.Scheduler).Return(fakeScheduler);

            return fakeJobContext;
        }


    }

    class MockedRetrieveFragmentJobBuilder:IMockedComponentBuilder<RetrieveFragmentJob>
    {
        public IDroneCommunicationService DroneCommunicationService { get; set; }
        public IDroneMailOporations MailOporations { get; set; }
        public IMailSender MailSender { get; set; }
        public MockedRetrieveFragmentJobBuilder()
        {
            DroneCommunicationService = MockRepository.GenerateStub<IDroneCommunicationService>();
            
            MailOporations = MockRepository.GenerateStub<IDroneMailOporations>();

            MailSender = MockRepository.GenerateStub<IMailSender>();
        }

        public MockedRetrieveFragmentJobBuilder WithFragmentResponse(FragmentResponse fragmentResponse = null )
        {
            if (fragmentResponse == null)
            {
                fragmentResponse = new FragmentResponse()
                                       {
                                           DroneSideOporations = new List<DroneSideOporationBase>(),
                                           EmailFragment = new EmailFragment()
                                       };

            }


            DroneCommunicationService.Stub(x => x.RetrieveFragment()).Return(fragmentResponse);

            return this;

        }

        public RetrieveFragmentJob Build()
        {
            return new RetrieveFragmentJob(DroneCommunicationService,MailOporations,MailSender);
        }
    }
}
