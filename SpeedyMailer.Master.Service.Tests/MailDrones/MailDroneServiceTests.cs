using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Master.Service.MailDrones;
using SpeedyMailer.Master.Service.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;

namespace SpeedyMailer.Master.Service.Tests.MailDrones
{
    [TestFixture]
    public class MailDroneServiceTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Wakeup_ShouldSetTheBaseUrlOfTheClientToTheBaseURLOfTheDrone()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.Execute<DroneStatus>(Arg<IRestRequest>
                                                              .Matches(
                                                                  m => m.Method == Method.POST &&
                                                                       m.Resource == mailDrone.WakeUpUri
                                                              ))).Repeat.Once().Return(new RestResponse<DroneStatus>()
                                                                                           {
                                                                                               Data = DroneStatus.Awake
                                                                                           });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            var mailDroneService = componentBuilder.Build();
            //Act
            mailDroneService.WakeUp(mailDrone);
            //Assert
            restClient.VerifyAllExpectations();

        }

        [Test]
        public void Wakeup_ShouldSendAPostWithTheWakeUpUriSuppliedByTheDrone()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();

            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>()
                            {
                                Data = DroneStatus.Awake
                            });

            restClient.Expect(x => x.BaseUrl).SetPropertyWithArgument(mailDrone.BaseUri);

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            var mailDroneService = componentBuilder.Build();
            //Act
            mailDroneService.WakeUp(mailDrone);
            //Assert
            restClient.VerifyAllExpectations();

        }

        [Test]
        public void Wakeup_ShouldReturnTheStatusFromTheRestCall()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>()
                {
                    Data = DroneStatus.Awake
                });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            var mailDroneService = componentBuilder.Build();
            //Act
            var status = mailDroneService.WakeUp(mailDrone);
            //Assert
            status.Should().Be(DroneStatus.Awake);

        }

        [Test]
        public void Wakeup_ShouldReturnErrorStatusIfReponseCodeIsError()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>()
                {
                    ResponseStatus = ResponseStatus.Error
                });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            var mailDroneService = componentBuilder.Build();
            //Act
            var status = mailDroneService.WakeUp(mailDrone);
            //Assert
            status.Should().Be(DroneStatus.ErrorOccured);

        }

        [Test]
        public void Wakeup_ShouldReturnNoConnectionIfResponseCodeIsTimedOut()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>()
                {
                    ResponseStatus = ResponseStatus.TimedOut
                });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            var mailDroneService = componentBuilder.Build();
            //Act
            var status = mailDroneService.WakeUp(mailDrone);
            //Assert
            status.Should().Be(DroneStatus.NoCommunication);

        }
    }

    public class MailDroneServiceMockedComponent:IMockedComponentBuilder<MailDroneService>
    {
        public IRestClient RestClient { get; set; }

        public MailDroneServiceMockedComponent()
        {
            RestClient = MockRepository.GenerateStub<IRestClient>();
            RestClient.Stub(x => x.Execute<DroneStatus>(Arg<RestRequest>.Is.Anything)).Return(new RestResponse<DroneStatus>()
                                                                                                  {
                                                                                                     Data = DroneStatus.Awake
                                                                                                  });
        }

        public MailDroneService Build()
        {
            return new MailDroneService(RestClient);    
        }
    }
}
