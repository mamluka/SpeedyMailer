using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Master.Service.MailDrones;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Master.Service.Tests.MailDrones
{
    [TestFixture]
    public class MailDroneServiceTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Wakeup_ShouldReturnErrorStatusIfReponseCodeIsError()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>
                            {
                                ResponseStatus = ResponseStatus.Error
                            });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            MailDroneService mailDroneService = componentBuilder.Build();
            //Act
            DroneStatus status = mailDroneService.WakeUp(mailDrone);
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
                .Return(new RestResponse<DroneStatus>
                            {
                                ResponseStatus = ResponseStatus.TimedOut
                            });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            MailDroneService mailDroneService = componentBuilder.Build();
            //Act
            DroneStatus status = mailDroneService.WakeUp(mailDrone);
            //Assert
            status.Should().Be(DroneStatus.NoCommunication);
        }

        [Test]
        public void Wakeup_ShouldReturnTheStatusFromTheRestCall()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>
                            {
                                Data = DroneStatus.Awake
                            });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            MailDroneService mailDroneService = componentBuilder.Build();
            //Act
            DroneStatus status = mailDroneService.WakeUp(mailDrone);
            //Assert
            status.Should().Be(DroneStatus.Awake);
        }

        [Test]
        public void Wakeup_ShouldSendAPostWithTheWakeUpUriSuppliedByTheDrone()
        {
            //Arrange
            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var restClient = MockRepository.GenerateMock<IRestClient>();

            restClient.Stub(x => x.Execute<DroneStatus>(Arg<IRestRequest>.Is.Anything)).Repeat.Once()
                .Return(new RestResponse<DroneStatus>
                            {
                                Data = DroneStatus.Awake
                            });

            restClient.Expect(x => x.BaseUrl).SetPropertyWithArgument(mailDrone.BaseUri);

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            MailDroneService mailDroneService = componentBuilder.Build();
            //Act
            mailDroneService.WakeUp(mailDrone);
            //Assert
            restClient.VerifyAllExpectations();
        }

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
                                                              ))).Repeat.Once().Return(new RestResponse<DroneStatus>
                                                                                           {
                                                                                               Data = DroneStatus.Awake
                                                                                           });

            var componentBuilder = new MailDroneServiceMockedComponent();
            componentBuilder.RestClient = restClient;

            MailDroneService mailDroneService = componentBuilder.Build();
            //Act
            mailDroneService.WakeUp(mailDrone);
            //Assert
            restClient.VerifyAllExpectations();
        }
    }
}