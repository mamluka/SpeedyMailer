using RestSharp;

namespace SpeedyMailer.Core.Apis
{
	public class Api
	{
		private ApiCall _apiCall;
		private readonly IRestClient _restClient;
		private readonly IApiCallsSettings _apiCallsSettings;

		public Api(IRestClient restClient,IApiCallsSettings apiCallsSettings)
		{
			_apiCallsSettings = apiCallsSettings;
			_restClient = restClient;
		}

		public T Call<T>() where T : ApiCall,IApiHost, new()
		{
			var apiCall = new T { ApiContext = this };
			_apiCall = apiCall;
			return apiCall;
		}

		public void Get()
		{
			ExecuteCall(Method.GET);
		}

		public void Post()
		{
			ExecuteCall(Method.POST);
		}

		private void ExecuteCall(Method method)
		{
			var restRequest = new RestRequest(_apiCall.Endpoint);
			
			if (method == Method.GET)
			{
				restRequest.AddObject(_apiCall.BoxedParameters);
			} else
			{
				restRequest.AddBody(_apiCall.BoxedParameters);
			}

			restRequest.RequestFormat = DataFormat.Json;
			restRequest.Method = method;

			_restClient.BaseUrl = _apiCallsSettings.ApiBaseUri;
			_restClient.Execute(restRequest);
		}
	}
}