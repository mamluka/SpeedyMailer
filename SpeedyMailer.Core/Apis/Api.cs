using System;
using RestSharp;

namespace SpeedyMailer.Core.Apis
{
	public class Api
	{
		private readonly IRestClient _restClient;
		private readonly IApiCallsSettings _apiCallsSettings;
		private string _apiBaseUrl;
		private ApiCall _apiCall { get; set; }

		public Api(IRestClient restClient,IApiCallsSettings apiCallsSettings)
		{
			_apiCallsSettings = apiCallsSettings;
			_restClient = restClient;
		}

		public IApiActions Call<T>(Action<T> action) where T : ApiCall, new()
		{
			var apiCall = new T();
			action(apiCall);
			_apiCall = apiCall;

			return new ApiActions(this);
		}

		public IApiActions Call<T>() where T : ApiCall, new()
		{
			var apiCall = new T();
			_apiCall = apiCall;
			return new ApiActions(this);
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

			if (_apiCall.BoxedParameters != null)
				HandleRequestBody(method, restRequest);

			
			restRequest.Method = method;

			_restClient.BaseUrl = GetBaseUrl();
			_restClient.Execute(restRequest);
		}

		private void HandleRequestBody(Method method, RestRequest restRequest)
		{
			if (method == Method.GET)
			{
				restRequest.AddObject(_apiCall.BoxedParameters);
			}
			else
			{
				restRequest.AddBody(_apiCall.BoxedParameters);
			}

			restRequest.RequestFormat = DataFormat.Json;
		}

		private string GetBaseUrl()
		{
			return _apiBaseUrl ?? _apiCallsSettings.ApiBaseUri;
		}

		public Api SetBaseUrl(string baseUrl)
		{
			_apiBaseUrl = baseUrl;
			return this;
		}
	}

	public interface IApiActions
	{
		void Get();
		void Post();
	}

	public class ApiActions : IApiActions
	{
		private readonly Api _apiHost;

		public ApiActions(Api apiHost)
		{
			_apiHost = apiHost;
		}

		public void Get()
		{
			_apiHost.Get();
		}

		public void Post()
		{
			_apiHost.Post();
		}
	}
}