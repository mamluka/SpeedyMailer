using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;

namespace SpeedyMailer.Core.Apis
{
	public class Api
	{
		private readonly IRestClient _restClient;
		private readonly ApiCallsSettings _apiCallsSettings;
		private string _apiBaseUrl;
		private IList<string> _requestFiles;

		public Api(IRestClient restClient, ApiCallsSettings apiCallsSettings)
		{
			_apiCallsSettings = apiCallsSettings;
			_restClient = restClient;
		}

		public void Call<TEndpoint>() where TEndpoint : ApiCall, new()
		{
			var apiCall = new TEndpoint();
			ExecuteCall(apiCall);
		}

		public void Call<TEndpoint>(Action<TEndpoint> action) where TEndpoint : ApiCall, new()
		{
			var apiCall = new TEndpoint();
			action(apiCall);
			ExecuteCall(apiCall);
		}

		public TResponse Call<TEndpoint, TResponse>()
			where TEndpoint : ApiCall, new()
			where TResponse : new()
		{
			var apiCall = new TEndpoint();
			return ExecuteCall<TResponse>(apiCall);
		}

		public TResponse Call<TEndpoint, TResponse>(Action<TEndpoint> action)
			where TEndpoint : ApiCall, new()
			where TResponse : new()
		{
			var apiCall = new TEndpoint();
			action(apiCall);
			return ExecuteCall<TResponse>(apiCall);
		}

		private TResponse ExecuteCall<TResponse>(ApiCall apiCall) where TResponse : new()
		{
			var restRequest = SetupApiCall(apiCall);

			Trace.WriteLine("Endpoint url called is: " + restRequest.Resource);
			Trace.WriteLine("Base url called is: " + _restClient.BaseUrl);

			var result = _restClient.Execute<TResponse>(restRequest);

			Trace.WriteLine("response status: " + result.ResponseStatus + " status code: " + result.StatusCode);
			Trace.WriteLine("Error message: " + result.ErrorMessage);

		    CleanUp();

			return result.Data;

		}

	    private void CleanUp()
	    {
	        _requestFiles = new List<string>();
	    }

	    private void ExecuteCall(ApiCall apiCall)
		{

			var restRequest = SetupApiCall(apiCall);

			Trace.WriteLine("Endpoint url called is: " + restRequest.Resource);
			Trace.WriteLine("Base url called is: " + _restClient.BaseUrl);

			var result = _restClient.Execute(restRequest);

			Trace.WriteLine("response status: " + result.ResponseStatus + " status code: " + result.StatusCode);
			Trace.WriteLine("Error message: " + result.ErrorMessage);

            CleanUp();
		}

		private RestRequest SetupApiCall(ApiCall apiCall)
		{
			var endpoint = ParseArguments(apiCall);
			var restRequest = new RestRequest(endpoint);

			var method = Translate(apiCall);
			restRequest.Method = method;

			if (_requestFiles != null)
				_requestFiles.ToList().ForEach(x => restRequest.AddFile(Path.GetFileName(x), x));

			HandleRequestBody(apiCall, restRequest);

			_restClient.BaseUrl = GetBaseUrl();
			return restRequest;
		}

		private string ParseArguments(ApiCall apiCall)
		{
			var endpoint = apiCall.Endpoint;
			var groups = Regex.Match(endpoint, "{(.+?)}").Groups[1];

			foreach (var capture in groups.Captures)
			{
				var parameter = capture.ToString();

				var value = apiCall
					.GetType()
					.GetProperties()
					.Single(x => x.Name.ToLower() == parameter)
					.GetValue(apiCall, null)
					.ToString()
					.ToLower();

				endpoint = endpoint.Replace("{" + parameter + "}", value);
			}

			return endpoint;
		}

		private static Method Translate(ApiCall apiCall)
		{
			switch (apiCall.CallMethod)
			{
				case RestMethod.Get:
					return Method.GET;
				case RestMethod.Post:
					return Method.POST;
				case RestMethod.Put:
					return Method.PUT;
				case RestMethod.Delete:
					return Method.DELETE;
				default:
					return Method.GET;
			}
		}

		private void HandleRequestBody(ApiCall apiCall, IRestRequest restRequest)
		{
			if (apiCall.CallMethod == RestMethod.Post)
			{
				restRequest.AddBody(apiCall);
			}
			else
			{
				restRequest.AddObject(apiCall);
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

		public Api AddFiles(IEnumerable<string> files)
		{
			_requestFiles = files.ToList();
			return this;
		}
	}

	public class VoidResponse
	{
	}
}