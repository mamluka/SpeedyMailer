using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Nancy.Testing;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.DataAccess.Drone;
using SpeedyMailer.Core.DataAccess.Fragments;
using SpeedyMailer.Master.Service.Emails;
using SpeedyMailer.Master.Service.MailDrones;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Service.Tests.Pool
{
    [TestFixture]
    public class PoolNancyModuleTests : AutoMapperAndFixtureBase
    {
        [TestFixtureSetUp]
        public void InitNancy()
        {
            Assembly.Load("SpeedyMailer.Master.Service");
        }

        [Test]
        public void RetrieveFragment_ShouldAddASleepOporationIfTheAreNoFragment()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<FragmenRequest>();


            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();


            var browser = new Browser(bootstrapper);
            //Act
            BrowserResponse result = browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            result.Body.DeserializeJson<FragmentResponse>().DroneSideOporations.Should().Contain(
                x => x.DroneSideOporationType == DroneSideOporationType.GoToSleep);
        }

        [Test]
        public void RetrieveFragment_ShouldExecuteTheEmailOporatioIfPresentInFragmentRequest()
        {
            //Arrange

            var fragment = Fixture.CreateAnonymous<FragmenRequest>();
            var emailOporations = MockRepository.GenerateMock<IPoolMailOporations>();
            emailOporations.Expect(
                x =>
                x.Preform(Arg<PoolSideOporationBase>.Matches(m => m.FragmentId == fragment.PoolSideOporation.FragmentId)))
                .Repeat.Once();

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.MailOporations = emailOporations;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            emailOporations.VerifyAllExpectations();
        }

        [Test]
        public void RetrieveFragment_ShouldPopAnEmailFromThePool()
        {
            //Arrange

            var fragment = Fixture.CreateAnonymous<FragmenRequest>();
            var fragmentRepository = MockRepository.GenerateMock<IFragmentRepository>();
            fragmentRepository.Expect(x => x.PopFragment()).Repeat.Once().Return(new EmailFragment());

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.FragmentRepository = fragmentRepository;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            fragmentRepository.VerifyAllExpectations();
        }

        [Test]
        public void RetrieveFragment_ShouldSetTheCurrentFragmentToTheResponseObject()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<FragmenRequest>();
            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();

            var browser = new Browser(bootstrapper);
            //Act
            BrowserResponse result = browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            result.Body.DeserializeJson<FragmentResponse>().EmailFragment.Should().NotBeNull();
        }

        [Test]
        public void RetrieveFragment_ShouldUpdateTheDroneRepWithTheCurrentDroneAsAsleepIfWeDontHaveAnyEmailFragments()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<FragmenRequest>();


            var mailDroneRep = MockRepository.GenerateMock<IMailDroneRepository>();
            mailDroneRep.Expect(x => x.Update(Arg<MailDrone>.Matches(m =>
                                                                     m.Status == DroneStatus.Asleep
                                                  ))).Repeat.Once();

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.MailDroneRepository = mailDroneRep;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            mailDroneRep.VerifyAllExpectations();
        }

        [Test]
        public void Update_ShouldLoadTheSleepListInTheStore()
        {
            //Arrange
            var sleepingDrones = new List<MailDrone>();
            sleepingDrones.AddMany(() => Fixture.CreateAnonymous<MailDrone>(), 3);

            var mailDroneRep = MockRepository.GenerateMock<IMailDroneRepository>();
            mailDroneRep.Expect(x => x.CurrentlySleepingDrones()).Repeat.Once().Return(sleepingDrones);

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.MailDroneRepository = mailDroneRep;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Post("/pool/update");
            //Assert
            mailDroneRep.VerifyAllExpectations();
        }

        [Test]
        public void Update_ShouldSendAWakeUpMessageToAllDrones()
        {
            //Arrange
            var sleepingDrones = new List<MailDrone>();
            sleepingDrones.AddMany(() => Fixture.CreateAnonymous<MailDrone>(), 3);

            var mailDroneRep = MockRepository.GenerateStub<IMailDroneRepository>();
            mailDroneRep.Stub(x => x.CurrentlySleepingDrones()).Repeat.Once().Return(sleepingDrones);

            var droneService = MockRepository.GenerateMock<IMailDroneService>();
            droneService.Expect(x => x.WakeUp(Arg<MailDrone>.Matches(m =>
                                                                     sleepingDrones.Any(p => p == m)
                                                  ))).Return(DroneStatus.Awake);

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.MailDroneRepository = mailDroneRep;
            bootstrapper.MailDroneService = droneService;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Post("/pool/update");
            //Assert
            droneService.VerifyAllExpectations();
        }

        [Test]
        public void Update_ShouldUpdateTheStatusOfTheDroneInTheStore()
        {
            //Arrange
            var sleepingDrones = new List<MailDrone>();
            sleepingDrones.AddMany(() => Fixture.CreateAnonymous<MailDrone>(), 3);

            var mailDroneRep = MockRepository.GenerateMock<IMailDroneRepository>();

            mailDroneRep.Stub(x => x.CurrentlySleepingDrones()).Repeat.Once().Return(sleepingDrones);

            mailDroneRep.Expect(x => x.Update(Arg<MailDrone>.Is.Anything))
                .Repeat
                .Times(sleepingDrones.Count);

            var droneService = MockRepository.GenerateStub<IMailDroneService>();
            droneService.Stub(x => x.WakeUp(Arg<MailDrone>.Is.Anything)).Return(DroneStatus.Awake);

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.MailDroneRepository = mailDroneRep;
            bootstrapper.MailDroneService = droneService;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Post("/pool/update");
            //Assert
            mailDroneRep.VerifyAllExpectations();
        }
    }
}