using RestSharp;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Master.Web.Core.Commands
{
	public class SendCreativeCommand:ApiCommand
	{
		private readonly IRestClient _restClient;
		private readonly ICreativeApisSettings _creativeApisSettings;
	    private readonly IBaseApiSettings _baseApiSettings;
	    public string CreativeId { get; set; }

	    public SendCreativeCommand(IRestClient restClient,IBaseApiSettings baseApiSettings,ICreativeApisSettings creativeApisSettings)
		{
		    _baseApiSettings = baseApiSettings;
		    _creativeApisSettings = creativeApisSettings;
			_restClient = restClient;
		}

        public override ApiResult Execute()
        {
            var restRequest = new RestRequest(_creativeApisSettings.AddCreative);
            var request = new CreativeApi.Add.Request
                              {
                                  CreativeId = CreativeId,
							  };

            restRequest.AddBody(request);
			restRequest.RequestFormat = DataFormat.Json;
			restRequest.Method = Method.POST;

            _restClient.BaseUrl = _baseApiSettings.ServiceBaseUrl;
            _restClient.Execute<FaultTolerantResponse>(restRequest);

        	return new ApiResult();
        }
	}

    public class FaultTolerantResponse
	{
		public bool ExceptionOccured;
		public string ExceptionMessage { get; set; }
		public string ExceptionName { get; set; }
	}

	public class FaultTolerantResponse<T>:FaultTolerantResponse
	{
		public T Model;
	}
}