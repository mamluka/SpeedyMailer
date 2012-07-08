using System;
using RestSharp;

namespace SpeedyMailer.Core.Apis
{
	public class Api
	{
		private readonly IRestClient _restClient;
		private readonly IApiCallsSettings _apiCallsSettings;
		private string _apiBaseUrl;
		private ApiCall ApiCall { get; set; }

		public Api(IRestClient restClient,IApiCallsSettings apiCallsSettings)
		{
			_apiCallsSettings = apiCallsSettings;
			_restClient = restClient;
		}

		public IApiActions Call<T>(Action<T> action) where T : ApiCall, new()
		{
			var apiCall = new T();
			action(apiCall);
			ApiCall = apiCall;

			return new ApiActions(this);
		}

		public IApiActions Call<T>() where T : ApiCall, new()
		{
			var apiCall = new T();
			ApiCall = apiCall;
			return new ApiActions(this);
		}

		public TResponse Get<TResponse>() where TResponse : new()
		{
			return ExecuteCall<TResponse>(Method.GET);
		}

		public TResponse Post<TResponse>() where TResponse : new()
		{
			return ExecuteCall<TResponse>(Method.POST);
		}

		private TResponse ExecuteCall<TResponse>(Method method) where TResponse : new()
		{
			var restRequest = new RestRequest(ApiCall.Endpoint);

			if (ApiCall.BoxedParameters != null)
				HandleRequestBody(method, restRequest);

			
			restRequest.Method = method;

			_restClient.BaseUrl = GetBaseUrl();
			return _restClient.Execute<TResponse>(restRequest).Data;
		}

		private void HandleRequestBody(Method method, IRestRequest restRequest)
		{
			if (method == Method.GET)
			{
				restRequest.AddObject(ApiCall.BoxedParameters);
			}
			else
			{
				restRequest.AddBody(ApiCall.BoxedParameters);
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
		T Get<T>() where T : new();
		void Post();
		T Post<T>() where T : new();
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
			_apiHost.Get<VoidResponse>();
		}

		public T Get<T>() where T : new()
		{
			return _apiHost.Get<T>();
		}

		public void Post()
		{
			_apiHost.Post<VoidResponse>();
		}

		public T Post<T>() where T : new()
		{
			return _apiHost.Post<T>();
		}
	}

	public class VoidResponse
	{
	}
}