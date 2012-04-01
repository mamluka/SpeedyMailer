using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Master.Service.MailDrones;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Master.Service.Tests.MailDrones
{
    public class MailDroneServiceMockedComponent : IMockedComponentBuilder<MailDroneService>
    {
        public MailDroneServiceMockedComponent()
        {
            RestClient = MockRepository.GenerateStub<IRestClient>();
            RestClient.Stub(x => x.Execute<DroneStatus>(Arg<RestRequest>.Is.Anything)).Return(new RestResponse
                                                                                                  <DroneStatus>
                                                                                                  {
                                                                                                      Data =
                                                                                                          DroneStatus.
                                                                                                          Awake
                                                                                                  });
        }

        public IRestClient RestClient { get; set; }


        public MailDroneService Build()
        {
            return new MailDroneService(RestClient);
        }

    }
}