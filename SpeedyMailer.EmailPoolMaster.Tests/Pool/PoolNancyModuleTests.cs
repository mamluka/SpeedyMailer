using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Nancy.Bootstrappers.Ninject;
using Nancy.Testing;
using Ninject;
using Ploeh.AutoFixture;
using Raven.Client;
using SpeedyMailer.Core.MailDrones;
using SpeedyMailer.EmailPoolMaster.MailDrones;
using SpeedyMailer.EmailPoolMaster.Pool;
using SpeedyMailer.EmailPoolMaster.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;

namespace SpeedyMailer.EmailPoolMaster.Tests.Pool
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
            mailDroneRep.Expect(x => x.SleepingDrones()).Repeat.Once().Return(sleepingDrones);

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
            mailDroneRep.Stub(x => x.SleepingDrones()).Repeat.Once().Return(sleepingDrones);

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

            mailDroneRep.Stub(x => x.SleepingDrones()).Repeat.Once().Return(sleepingDrones);

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
        public void RetrieveFragment_ShouldUpdateTheMailToCompleteStatusInTheStoreIfMailIdIsFGiven()
        {
            //Arrange

            var fragment = Fixture.CreateAnonymous<FragmenRequest>();

            var bootstrapper = new MyNinjectBootstrapperWithMockedObjects();
            

            var browser = new Browser(bootstrapper);

            //Act
            var result = browser.Get("/pool/retrievefragment", with => with.JsonBody(fragment));
            //Assert
            Assert.Fail();
        }
    }

    public class MyNinjectBootstrapperWithMockedObjects : NinjectNancyBootstrapper
    {
        public IMailDroneRepository MailDroneRepository { get; set; }
        public IMailDroneService MailDroneService { get; set; }

        public MyNinjectBootstrapperWithMockedObjects()
        {
            MailDroneService = MockRepository.GenerateStub<IMailDroneService>();
            MailDroneRepository = MockRepository.GenerateStub<IMailDroneRepository>();
        }

        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            existingContainer.Bind<IMailDroneRepository>().ToConstant(MailDroneRepository);
            existingContainer.Bind<IMailDroneService>().ToConstant(MailDroneService);
        }
    }
}
