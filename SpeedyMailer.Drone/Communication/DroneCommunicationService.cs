using Nancy.Json;
using Newtonsoft.Json;
using RestSharp;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Master.Web.UI.Configurations;

namespace SpeedyMailer.Master.Web.UI.Communication
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


            RestResponse<FragmentResponse> response = restClient.Execute<FragmentResponse>(request);

            var fragment = JsonConvert.DeserializeObject<FragmentResponse>(response.Content);


            return fragment;
        }

    }
}