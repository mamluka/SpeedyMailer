using Nancy.Json;
using Newtonsoft.Json;
using RestSharp;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Drone.Configurations;

namespace SpeedyMailer.Drone.Communication
{
    public class DroneCommunicationService : IDroneCommunicationService
    {
        private readonly IDroneConfigurationManager configurationManager;
        private readonly IRestClient restClient;

        public DroneCommunicationService(IRestClient restClient, IDroneConfigurationManager configurationManager)
        {
            this.restClient = restClient;
            this.configurationManager = configurationManager;
        }


        public FragmentResponse RetrieveFragment()
        {
            restClient.BaseUrl = configurationManager.BasePoolUrl;

            var request = new RestRequest
                              {
                                  Resource = configurationManager.PoolOporationsUrls.PopFragmentUrl
                              };

            JsonSettings.MaxJsonLength = 1000000;
            request.RequestFormat = DataFormat.Json;


            var response = restClient.Execute<FragmentResponse>(request);

            var fragment = JsonConvert.DeserializeObject<FragmentResponse>(response.Content);


            return fragment;
        }

    }
}