using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Master.Web.UI.Communication;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Communication
{
    [TestFixture]
    public class DroneCommunicationServiceTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Pop_ShouldDeserializeTheResponseAndReturnTheFragment()
        {
            //Arrange
            var fragmentResponse = Fixture.CreateAnonymous<FragmentResponse>();
            var restResponse = MockRepository.GenerateStub<RestResponse<FragmentResponse>>();

            restResponse.Data = fragmentResponse;

            var restClient = MockRepository.GenerateStub<IRestClient>();
            restClient.Stub(x => x.Execute<FragmentResponse>(Arg<RestRequest>.Is.Anything)).Return(restResponse);

            var builder = new MockedDroneCommunicationServiceBuilder(Fixture);
            builder.RestClient = restClient;
            //Act
            DroneCommunicationService droneCommunicationService = builder.Build();
            //Assert
            FragmentResponse fragment = droneCommunicationService.RetrieveFragment();

            fragment.Should().BeSameAs(fragmentResponse);
        }

        [Test]
        public void Pop_ShouldLoadTheBaseURLToTheRestClient()
        {
            //Arrange
            var baseUrl = Fixture.CreateAnonymous<string>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.BaseUrl).SetPropertyWithArgument(baseUrl);


            var builder = new MockedDroneCommunicationServiceBuilder(Fixture);
            builder.RestClient = restClient;

            DroneCommunicationService droneCommunicationService =
                builder.WithBasePoolUrl(baseUrl).AddResponseMockedObject().Build();
            //Act
            droneCommunicationService.RetrieveFragment();
            //Assert
            restClient.VerifyAllExpectations();
        }

        [Test]
        public void Pop_ShouldLoadThePopFragmentUrlIntoTheRequestAndExecuteTheRequest()
        {
            //Arrange
            var fragmentUrl = Fixture.CreateAnonymous<string>();

            var fragmentResponse = Fixture.CreateAnonymous<FragmentResponse>();
            var restResponse = MockRepository.GenerateStub<RestResponse<FragmentResponse>>();
            restResponse.Data = fragmentResponse;

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.Execute<FragmentResponse>(Arg<RestRequest>.Matches(m =>
                                                                                        m.Resource == fragmentUrl)))
                .Repeat.Once().Return(restResponse);

            var builder = new MockedDroneCommunicationServiceBuilder(Fixture);
            builder.RestClient = restClient;

            DroneCommunicationService droneCommunicationService = builder.WithPopFragmentUrl(fragmentUrl).Build();
            //Act
            droneCommunicationService.RetrieveFragment();
            //Assert
            restClient.VerifyAllExpectations();
        }
    }
}