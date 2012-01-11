using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Nancy.Bootstrappers.Ninject;
using Nancy.Testing;
using Ninject;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.MailDrones;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.Master.MailDrones;
using SpeedyMailer.EmailPool.Master.Pool;
using SpeedyMailer.EmailPool.Master.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;

namespace SpeedyMailer.EmailPool.Master.Tests.Pool
{
    [TestFixture]
    public class PoolNancyModuleTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        

        [TestFixtureSetUp]
        public void InitNancy()
        {

            Assembly.Load("SpeedyMailer.EmailPoolMaster");
        }

        [Test]
        public void Update_ShouldLoadTheSleepListInTheStore()
        {
            //Arrange
            var sleepingDrones = new List<MailDrone>();
            sleepingDrones.AddMany(() => Fixture.CreateAnonymous<MailDrone>(),3);

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

        [Test]
        public void RetrieveFragment_ShouldExecuteTheEmailOporatioIfPresentInFragmentRequest()
        {
            //Arrange

            var fragment = Fixture.CreateAnonymous<FragmenRequest>();
            var emailOporations = MockRepository.GenerateMock<IEMailOporations>();
            emailOporations.Expect(x => x.Preform(Arg<PoolSideOporationBase>.Matches(m => m.FragmentId == fragment.PoolSideOporation.FragmentId))).Repeat.Once();

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.EMailOporations = emailOporations;

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
            var emailPool = MockRepository.GenerateMock<IEmailPoolService>();
            emailPool.Expect(x => x.PopEmail()).Repeat.Once().Return(new EmailFragment());

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.EmailPoolService = emailPool;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            emailPool.VerifyAllExpectations();
        }

        [Test]
        public void RetrieveFragment_ShouldUpdateTheDroneRepWithTheCurrentDroneAsAsleepIfWeDontHaveAnyEmailFragments()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<FragmenRequest>();

            var emailPool = MockRepository.GenerateStub<IEmailPoolService>();
            emailPool.Stub(x => x.PopEmail()).Repeat.Once().Return(null);

            var mailDroneRep = MockRepository.GenerateMock<IMailDroneRepository>();
            mailDroneRep.Expect(x => x.Update(Arg<MailDrone>.Matches(m => 
                                                                    m.Status == DroneStatus.Asleep
                                                  ))).Repeat.Once();

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.EmailPoolService = emailPool;
            bootstrapper.MailDroneRepository = mailDroneRep;

            var browser = new Browser(bootstrapper);

            //Act
            browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            mailDroneRep.VerifyAllExpectations();
        }

        [Test]
        public void RetrieveFragment_ShouldSetTheCurrentFragmentToTheResponseObject()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<FragmenRequest>();
            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();

            var browser = new Browser(bootstrapper);
            //Act
            var result = browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            result.Body.DeserializeJson<FragmentResponse>().EmailFragment.Should().NotBeNull();
        }

        [Test]
        public void RetrieveFragment_ShouldAddASleepOporationIfTheAreNoFragment()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<FragmenRequest>();

            var emailPool = MockRepository.GenerateStub<IEmailPoolService>();
            emailPool.Stub(x => x.PopEmail()).Return(null);
                

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            bootstrapper.EmailPoolService = emailPool;

            var browser = new Browser(bootstrapper);
            //Act
            var result = browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            result.Body.DeserializeJson<FragmentResponse>().DroneSideOporations.Should().Contain(
                x => x.DroneSideOporationType == DroneSideOporationType.PutAsleep);
        }



    }

    public class MyNinjectBootstrapperWithMockedObjects : NinjectNancyBootstrapper
    {
        public IMailDroneRepository MailDroneRepository { get; set; }
        public IMailDroneService MailDroneService { get; set; }
        public IEMailOporations EMailOporations { get; set; }
        public IEmailPoolService EmailPoolService { get; set; }

        public MyNinjectBootstrapperWithMockedObjects()
        {
            MailDroneService = MockRepository.GenerateStub<IMailDroneService>();
            MailDroneRepository = MockRepository.GenerateStub<IMailDroneRepository>();
            EMailOporations = MockRepository.GenerateStub<IEMailOporations>();
            EmailPoolService = MockRepository.GenerateStub<IEmailPoolService>();

            EmailPoolService.Stub(x => x.PopEmail()).Return(new EmailFragment());
        }

        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            existingContainer.Bind<IMailDroneRepository>().ToConstant(MailDroneRepository);
            existingContainer.Bind<IMailDroneService>().ToConstant(MailDroneService);
            existingContainer.Bind<IEMailOporations>().ToConstant(EMailOporations);
            existingContainer.Bind<IEmailPoolService>().ToConstant(EmailPoolService);
        }
    }
}
