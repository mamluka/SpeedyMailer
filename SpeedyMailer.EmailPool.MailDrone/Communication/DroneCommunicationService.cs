using RestSharp;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.MailDrone.Configurations;

namespace SpeedyMailer.EmailPool.MailDrone.Communication
{
    public class DroneCommunicationService : IDroneCommunicationService
    {
        private readonly IRestClient restClient;
        private readonly IDroneConfigurationManager configurationManager;

        public DroneCommunicationService(IRestClient restClient, IDroneConfigurationManager configurationManager)
        {
            this.restClient = restClient;
            this.configurationManager = configurationManager;
        }

        public FragmentResponse RetrieveFragment()
        {
            restClient.BaseUrl = configurationManager.BasePoolUrl;

            var request = new RestRequest()
                              {
                                  Resource = configurationManager.PoolOporationsUrls.PopFragmentUrl
                              };

            var response = restClient.Execute<FragmentResponse>(request);

            return response.Data;
        }
    }
}