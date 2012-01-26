using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.MailDrone.Communication;
using SpeedyMailer.EmailPool.MailDrone.Configurations;
using SpeedyMailer.MailDrone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using SpeedyMailer.MailDrone;
using FluentAssertions;

namespace SpeedyMailer.MailDrone.Tests.Communication
{
    [TestFixture]
    public class DroneCommunicationServiceTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Pop_ShouldLoadTheBaseURLToTheRestClient()
        {
            //Arrange
            var baseUrl = Fixture.CreateAnonymous<string>();

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.BaseUrl).SetPropertyWithArgument(baseUrl);

            

            var builder = new MockedDroneCommunicationServiceBuilder(Fixture);
            builder.RestClient = restClient;

            var droneCommunicationService = builder.WithBasePoolUrl(baseUrl).AddResponseMockedObject().Build();
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

            var droneCommunicationService = builder.WithPopFragmentUrl(fragmentUrl).Build();
            //Act
            droneCommunicationService.RetrieveFragment();
            //Assert
            restClient.VerifyAllExpectations();

        }

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
            var droneCommunicationService = builder.Build();
            //Assert
            var fragment = droneCommunicationService.RetrieveFragment();

            fragment.Should().BeSameAs(fragmentResponse);
        }

       

    }

    public class MockedDroneCommunicationServiceBuilder : IMockedComponentBuilder<DroneCommunicationService>
    {
        private readonly IFixture fixture;
        public IRestClient RestClient { get; set; }
        public IDroneConfigurationManager DroneConfigurationManager { get; set; }

        public MockedDroneCommunicationServiceBuilder(IFixture fixture)
        {

            this.fixture = fixture;

           DroneConfigurationManager = MockRepository.GenerateStub<IDroneConfigurationManager>();
            DroneConfigurationManager.BasePoolUrl = "baseurl";
            DroneConfigurationManager.PoolOporationsUrls =
                fixture.Build<PoolOporationsUrls>().With(x => x.PopFragmentUrl, "fragmenturl").CreateAnonymous();
        }

        public MockedDroneCommunicationServiceBuilder WithBasePoolUrl(string url)
        {
            DroneConfigurationManager.BasePoolUrl = url;
            return this;
        }

        public MockedDroneCommunicationServiceBuilder WithPopFragmentUrl(string url)
        {
            DroneConfigurationManager.PoolOporationsUrls =
                fixture.Build<PoolOporationsUrls>().With(x => x.PopFragmentUrl, url).CreateAnonymous();
            return this;
        }

        public MockedDroneCommunicationServiceBuilder AddResponseMockedObject()
        {
            var fragmentResponse = fixture.CreateAnonymous<FragmentResponse>();
            var restResponse = MockRepository.GenerateStub<RestResponse<FragmentResponse>>();
            restResponse.Data = fragmentResponse;

            RestClient.Stub(x => x.Execute<FragmentResponse>(Arg<RestRequest>.Is.Anything)).Return(restResponse);
            return this;
        }

        public DroneCommunicationService Build()
        {
            return new DroneCommunicationService(RestClient,DroneConfigurationManager);
        }
    }
}
