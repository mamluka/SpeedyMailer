using System.Collections.Generic;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Quartz;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Drone.Communication;
using SpeedyMailer.Drone.Mail;
using SpeedyMailer.Drone.Tasks;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Mail
{
    [TestFixture]
    public class RetrieveFragmentJobTests : AutoMapperAndFixtureBase
    {
        private IJobExecutionContext FakeContext()
        {
            var fakeScheduler = MockRepository.GenerateStub<IScheduler>();

            var fakeJobContext = MockRepository.GenerateStub<IJobExecutionContext>();
            fakeJobContext.Stub(x => x.Scheduler).Return(fakeScheduler);

            return fakeJobContext;
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
            RetrieveFragmentJob job = builder.WithFragmentResponse().Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            fakeScheduler.VerifyAllExpectations();
        }

        [Test]
        public void Execute_ShouldExecuteTheFragmentOporations()
        {
            //Arrange
            IJobExecutionContext fakeJobContext = FakeContext();

            var fragmentReponse = Fixture.CreateAnonymous<FragmentResponse>();
            fragmentReponse.DroneSideOporations = new List<DroneSideOporationBase>
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

            RetrieveFragmentJob job = builder.WithFragmentResponse(fragmentReponse).Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            mailOporation.VerifyAllExpectations();
        }

        [Test]
        public void Execute_ShouldGetTheMailFragmentUsingTheDroneCommunication()
        {
            //Arrange
            IJobExecutionContext fakeJobContext = FakeContext();

            var fragmentReponse = Fixture.CreateAnonymous<FragmentResponse>();

            var droneCom = MockRepository.GenerateMock<IDroneCommunicationService>();
            droneCom.Expect(x => x.RetrieveFragment()).Repeat.Once().Return(fragmentReponse);

            var builder = new MockedRetrieveFragmentJobBuilder();
            builder.DroneCommunicationService = droneCom;
            RetrieveFragmentJob job = builder.Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            droneCom.VerifyAllExpectations();
        }

        [Test]
        public void Execute_ShouldSendTheEmailFragment()
        {
            //Arrange
            IJobExecutionContext fakeJobContext = FakeContext();

            var fragmentReponse = Fixture.CreateAnonymous<FragmentResponse>();

            var mailSender = MockRepository.GenerateMock<IMailSender>();
            mailSender.Expect(x => x.ProcessFragment(Arg<EmailFragment>.Is.Equal(fragmentReponse.EmailFragment))).Repeat
                .Once();


            var builder = new MockedRetrieveFragmentJobBuilder();
            builder.MailSender = mailSender;

            RetrieveFragmentJob job = builder.WithFragmentResponse(fragmentReponse).Build();
            //Act
            job.Execute(fakeJobContext);
            //Assert
            mailSender.VerifyAllExpectations();
        }
    }
}