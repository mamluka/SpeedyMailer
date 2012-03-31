using Ploeh.AutoFixture;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Master.Web.UI.Communication;
using SpeedyMailer.Master.Web.UI.Configurations;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Drone.Tests.Communication
{
    public class MockedDroneCommunicationServiceBuilder : IMockedComponentBuilder<DroneCommunicationService>
    {
        private readonly IFixture fixture;

        public MockedDroneCommunicationServiceBuilder(IFixture fixture)
        {
            this.fixture = fixture;

            DroneConfigurationManager = MockRepository.GenerateStub<IDroneConfigurationManager>();
            DroneConfigurationManager.BasePoolUrl = "baseurl";
            DroneConfigurationManager.PoolOporationsUrls =
                fixture.Build<PoolOporationsUrls>().With(x => x.PopFragmentUrl, "fragmenturl").CreateAnonymous();
        }

        public IRestClient RestClient { get; set; }
        public IDroneConfigurationManager DroneConfigurationManager { get; set; }

        #region IMockedComponentBuilder<DroneCommunicationService> Members

        public DroneCommunicationService Build()
        {
            return new DroneCommunicationService(RestClient, DroneConfigurationManager);
        }

        #endregion

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
    }
}